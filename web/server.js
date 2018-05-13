const express = require('express');
const app = express();
const fs = require('fs');
const bodyParser = require('body-parser');
const urlencodedParser = bodyParser.urlencoded({ extended: false });
const uuidv1 = require('uuid/v1');
const cookieParser = require('cookie-parser');
const jwt = require('jsonwebtoken');

const auth = require('./auth');
const azureapi = require('./azureapi');

const redirect_uri = 'http%3a%2f%2flocalhost:3000%2fapp%2f';

app.use(cookieParser());
app.use(express.static('public'));

app.get('/', (req, res) => {
  res.sendFile(__dirname + '/index.html');
});

app.get('/app', (req, res) => {

    var existingToken = req.cookies['token'];
    if(existingToken && existingToken.length > 0) {
        res.sendFile(__dirname + '/app.html');
    } else {

        var nonce = uuidv1();

        // TODO!: Make that cookie secure when we have HTTPS figured out
        res.cookie('nonce', nonce, {expire: new Date(), httpOnly: true });

        res.redirect('https://login.microsoftonline.com/common/oauth2/authorize?client_id=' + auth.client_id +
            '&response_mode=form_post&response_type=id_token+code' +
            '&scope=openid%2cemail%2cprofile' +
            '&redirect_uri=' + redirect_uri +
            '&resource=https%3A%2F%2Fmanagement.azure.com%2F' +
            '&post_logout_redirect_uri=' + redirect_uri +
            '&nonce=' + nonce);
    }
});

app.post('/app', urlencodedParser, (req, res) => {
    if (!req.body) return res.sendStatus(400);

    const rawToken = req.body.id_token;
    const rawCode = req.body.code;

    var decodedNotVerified = jwt.decode(rawToken, {complete: true});
    auth.fetchCert(decodedNotVerified.header.kid, (cert) => {
        if(cert.length === 0) {
            console.log('no cert for verifying jwt');
            return res.sendStatus(400);
        }

        jwt.verify(rawToken, auth.formatCert(cert), (err, token) => {
            if (err) {
                console.log(err);
                return res.sendStatus(400);
            }

            if (!token.nonce || !token.nonce.length > 0 || req.cookies['nonce'] != token.nonce ) {
                console.log('nonce mismatch');
                return res.sendStatus(400);
            }

            // now that the token is validated - use the 'code' to get a token that we can use for azure mgmt
            auth.fetchMgmtToken(rawCode, (mgmtToken) => {

                 // TODO: Make this cookie https only
                res.cookie('token', mgmtToken, {expire: new Date((new Date()).getTime() + 60*60*1000), httpOnly: true });

                res.sendFile(__dirname + '/app.html');
            });
        });
    });
});

app.get('/api/test', (req, res) => {
    azureapi.fetchStorageResources(req.cookies['token'], 'eas3d14-e16a-49da-9141-e522cf579e7a', (data) => {
        res.json(data);
    });
});

app.get('/api/subscriptions', (req, res) => {
    azureapi.fetchSubscriptions(req.cookies['token'], (data) => {

        var subscriptions = data.value.map(sub => {
            return {
                id: sub.subscriptionId,
                name: sub.displayName
            };
        });

        res.json({
            subscriptions: subscriptions
        });
    });
});

app.get('/api/accounts/:subscriptionid', (req, res) => {

    azureapi.fetchStorageResources(req.cookies['token'], req.params.subscriptionid, (data) => {
        var accounts = data.value.map(account => {
            return {
                id: account.id,
                name: account.name
            };
        });

        res.json({
            accounts: accounts
        });
    });
});

app.listen(process.env.PORT || 3000, () => {
    console.log('listening on port ' + (process.env.PORT || 3000));
});
