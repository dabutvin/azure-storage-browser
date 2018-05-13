const request = require('request');

exports.fetchSubscriptions = function(token, callback) {
    request({
        url: 'https://management.azure.com/subscriptions?api-version=2015-01-01',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    }, (err, res, body) => {
        if (err) {
            console.log(err);
            callback(err);
        }

        callback(JSON.parse(body));
    });
};

exports.fetchStorageResources = function (token, subscriptionId, callback) {
    request({
        url: 'https://management.azure.com/subscriptions/' +
                subscriptionId +
                '/resources?api-version=2017-05-10&$filter=resourceType eq \'Microsoft.Storage/storageAccounts\'',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    }, (err, res, body) => {
        if (err) {
            console.log(err);
            callback(err);
        }

        callback(JSON.parse(body));
    });
};

exports.fetchStorageKey = function (token, id, callback) {
    request.post({
        url: 'https://management.azure.com' + id + '/listKeys?api-version=2017-06-01',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    }, (err, res, body) => {
        if (err) {
            console.log(err);
            callback(err);
        }

        callback(JSON.parse(body));
    })
};
