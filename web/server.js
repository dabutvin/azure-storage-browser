const express = require('express');
const app = express();
const fs = require('fs');
const bodyParser = require('body-parser');
const urlencodedParser = bodyParser.urlencoded({ extended: false });
const uuidv1 = require('uuid/v1');
const cookieParser = require('cookie-parser');
const jwt = require('jsonwebtoken');
const request = require('request');

const client_id = '94e776f0-1861-4e75-a7dc-bbad87906169';
const redirect_uri = 'http%3a%2f%2flocalhost:3000%2fapp%2f';

var keys = [];

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

        res.redirect('https://login.microsoftonline.com/common/oauth2/authorize?client_id=' + client_id +
            '&response_mode=form_post&response_type=id_token+code&scope=openid%2cemail%2cprofile&redirect_uri=' + redirect_uri +
            '&post_logout_redirect_uri=' + redirect_uri +
            '&nonce=' + nonce);
    }
});

app.post('/app', urlencodedParser, (req, res) => {
    if (!req.body) return res.sendStatus(400);

    const rawToken = req.body.id_token;

    var decodedNotVerified = jwt.decode(rawToken, {complete: true});
    fetchCert(decodedNotVerified.header.kid, (cert) => {
        if(cert.length === 0) {
            console.log('no cert for verifying jwt');
            return res.sendStatus(400);
        }

        jwt.verify(rawToken, formatCert(cert), (err, token) => {
            if (err) {
                console.log(err);
                return res.sendStatus(400);
            }

            if (!token.nonce || !token.nonce.length > 0 || req.cookies['nonce'] != token.nonce ) {
                console.log('nonce mismatch');
                return res.sendStatus(400);
            }

            // TODO: Make this cookie https only
            res.cookie('token', rawToken, {expire: new Date((new Date()).getTime() + 60*60*1000), httpOnly: true });

            res.sendFile(__dirname + '/app.html');
        });
    });


});

app.get('/api/subscriptions', (req, res) => {

    res.json({
        subscriptions: [
        {
            id: '12345',
            name: 'Ultimate'
        },
        {
            id: '67890',
            name: 'Mock'
        }]
    });
});

app.get('/api/accounts/:subscriptionid', (req, res) => {
    res.json({
        accounts: [
        {
            id: '1231231' + req.params.subscriptionid,
            name: 'ford: ' + req.params.subscriptionid
        }]
    });
});

app.listen(process.env.PORT || 3000, () => {
    console.log('listening on port ' + (process.env.PORT || 3000));
});

function fetchCert(kid, callback) {
    if (keys.length === 0) {
        request('https://login.microsoftonline.com/common/discovery/v2.0/keys', (err, res, body) => {
            if(err) {
                console.log(err);
                callback('');
            }

            keys = JSON.parse(body).keys;
            callback(pickCert(kid));
        });
    } else {
        callback(pickCert(kid));
    }
}

function pickCert(kid) {
    // use kid from jwt to pick x5c
    // https://tools.ietf.org/html/rfc7517#section-4.5
    for (var i = 0; i < keys.length; i++) {
        if (keys[i].kid === kid) {
            return keys[i].x5c;
        }
    }

    return '';
}

function formatCert(cert) {
    // see https://developer.byu.edu/docs/consume-api/use-api/implement-openid-connect/jwks-public-key-documentation
    var result = '-----BEGIN CERTIFICATE-----\n';

    while (cert.length > 64) {
        result += cert.slice(0, 64) + '\n';
        cert = cert.slice(64);
    }

    result += cert;
    result += '\n-----END CERTIFICATE-----';

    return result;
}

