const request = require('request');

exports.fetchSubscriptions = function(token, callback) {
    request({
        url: 'https://management.azure.com/subscriptions?api-version=2015-01-01',
        headers: {
            'Authorization': 'Bearer ' + token
        }
    }, (err, res, body) => {
        if(err) {
            console.log(err);
            callback(err);
        }

        callback(res.body);
    });
};
