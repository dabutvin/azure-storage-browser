const express = require('express');
const app = express();
const fs = require('fs');


app.use(express.static('public'));

app.get('/', (req, res) => {
  res.sendFile(__dirname + '/index.html');
});

app.get('/app', (req, res) => {
    res.sendFile(__dirname + '/app.html');
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