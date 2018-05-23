const request = require('request');
const secrets = require('./secrets');

var keys = [];
const client_id = '46f5f6ef-c1dc-40bd-9aa8-1e044c067ab4';

function fetchCert (kid, callback) {
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

function pickCert (kid) {
    // use kid from jwt to pick x5c
    // https://tools.ietf.org/html/rfc7517#section-4.5
    for (var i = 0; i < keys.length; i++) {
        if (keys[i].kid === kid) {
            return keys[i].x5c;
        }
    }

    return '';
}

function formatCert (cert) {
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

/*
* returns {
  "token_type": "Bearer",
  "scope": "user_impersonation",
  "expires_in": "3599",
  "ext_expires_in": "0",
  "expires_on": "1526919924",
  "not_before": "1526916024",
  "resource": "https://management.azure.com/",
  "access_token": "eyJ0eXdhKV...,
  "refresh_token": "AQABAsdfAADX...,
  "id_token": "eyJ0esdfKV1QiLCJ..."
}
*/
function fetchMgmtTokens(code, callback) {
    request.post({
        url: 'https://login.microsoftonline.com/common/oauth2/token',
        form: {
            client_id: client_id,
            scope: 'https://management.azure.com/read',
            redirect_uri: 'http://localhost:3000/app/',
            grant_type: 'authorization_code',
            client_secret: secrets.client_secret,
            code: code
        }
    }, (err, res, body) => {
        if (err) {
            console.log(err);
            callback(err);
        } else {
            callback(JSON.parse(body));
        }
    });
}

/*
*
* returns {
  "token_type": "Bearer",
  "scope": "user_impersonation",
  "expires_in": "3600",
  "ext_expires_in": "0",
  "expires_on": "1527047345",
  "not_before": "1527043445",
  "resource": "https://management.azure.com/",
  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciO...",
  "refresh_token": "AQABAAAAAADX8GCi6Js6SK82TsD..."
}
*/
function refreshMgmtTokens (refresh_token, callback) {
    request.post({
        url: 'https://login.microsoftonline.com/common/oauth2/token',
        form: {
            client_id: client_id,
            scope: 'https://management.azure.com/read',
            redirect_uri: 'http://localhost:3000/app/',
            grant_type: 'refresh_token',
            client_secret: secrets.client_secret,
            refresh_token: refresh_token
        }
    }, (err, res, body) => {
        if (err) {
            console.log(err);
            callback(err);
        } else {
            callback(JSON.parse(body));
        }
    });
}

exports.client_id = client_id;
exports.fetchCert = fetchCert;
exports.formatCert = formatCert;
exports.fetchMgmtTokens = fetchMgmtTokens;
exports.refreshMgmtTokens = refreshMgmtTokens;
