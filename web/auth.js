const request = require('request');
const secrets = require('./secrets');

var keys = [];
const client_id = '46f5f6ef-c1dc-40bd-9aa8-1e044c067ab4';

exports.client_id = client_id;

exports.fetchCert = function(kid, callback) {
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
};

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

exports.formatCert = function(cert) {
    // see https://developer.byu.edu/docs/consume-api/use-api/implement-openid-connect/jwks-public-key-documentation
    var result = '-----BEGIN CERTIFICATE-----\n';

    while (cert.length > 64) {
        result += cert.slice(0, 64) + '\n';
        cert = cert.slice(64);
    }

    result += cert;
    result += '\n-----END CERTIFICATE-----';

    return result;
};

exports.fetchMgmtToken = function(code, callback) {
    request.post({
        url: 'https://login.microsoftonline.com/common/oauth2/token',
        form: {
            client_id: client_id,
            scope: 'https://management.azure.com/read',
            code: code,
            redirect_uri: 'http://localhost:3000/app/',
            grant_type: 'authorization_code',
            client_secret: secrets.client_secret
        }
    }, (err, res, body) => {
        if(err) {
            console.log(err);
            callback(err);
        }

        callback(JSON.parse(body).access_token);
    });
};
