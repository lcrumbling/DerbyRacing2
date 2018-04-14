'use strict';

const Hapi = require('hapi');

const server = new Hapi.Server();
server.connection({ port: 2000 });

var io = require('socket.io')(server.listener);

io.on('connection', function (socket) {
    socket.join('racer-data');
});

server.register(require('inert'), (err) => {

    if (err) {
        throw err;
    }

    server.route({
        method: 'GET',
        path: '/{param*}',
        handler: {
            directory: {
                path: 'public',
                index: true
            }
        }
    });

    server.route({
        method: 'GET',
        path: '/apiracercompleted',
        handler: function (request, reply) {
            var obj = {};
            obj['track'] = request.query.track;
            obj['time'] = request.query.time;
            io.to('racer-data').emit('racercompleted', obj);
            reply('OK');
        }
    });

    server.route({
        method: 'GET',
        path: '/apiheatchanged',
        handler: function (request, reply) {
            var obj = {};
            console.log(request.query);
            console.log(request.query.heatnum);
            console.log(request.query.racer1);

            obj['heatnum'] = request.query.heatnum;
            obj['totalheats'] = request.query.totalheats;
            obj['racer1'] = request.query.racer1;
            obj['racer2'] = request.query.racer2;
            obj['racer3'] = request.query.racer3;
            obj['racer4'] = request.query.racer4;
            obj['nextracer1'] = request.query.nextracer1;
            obj['nextracer2'] = request.query.nextracer2;
            obj['nextracer3'] = request.query.nextracer3;
            obj['nextracer4'] = request.query.nextracer4;
            io.to('racer-data').emit('heatchanged', obj);
            reply('OK');
        }
    });

    server.start(function (err) {
        if (err) {
            throw err;
        }

        console.log('Server running at: ' + server.info.uri);
    });
});
