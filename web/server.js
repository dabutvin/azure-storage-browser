var http = require('http');
var fs = require('fs');
var path = require('path');

http.createServer(function (request, response) {
    console.log('request ', request.url);

    fs.readFile('./index.html', function(error, content) {
        if (error) {
            response.writeHead(500);
            response.end('Sorry :(: ' + error.code);
            response.end();
        } else {
            response.writeHead(200, { 'Content-Type': 'text/html' });
            response.end(content, 'utf-8');
        }
    });

}).listen(process.env.PORT || 3000);
