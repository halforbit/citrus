
$.fn.serializeObject = function ()
{
    var o = {};

    var a = this.serializeArray();

    $.each(a, function ()
    {
        if (o[this.name] !== undefined)
        {
            if (!o[this.name].push)
            {
                o[this.name] = [o[this.name]];
            }

            o[this.name].push(this.value || '');

        }
        else
        {
            o[this.name] = this.value || '';
        }
    });

    return o;
};

//

var Client =
{
    loadLocationHash: function()
    {
        Client.load(location.hash.substring(1), 'main');
    },

    load: function (url, container, respond)
    {
        var me = this;

        var model = null;

        var viewSource = null;

        async.parallel(
            [
                function (respond)
                {
                    Client.getModel(url, function (error, data)
                    {
                        if (error)
                        {
                            respond(error);

                            return;
                        }

                        model = data;

                        respond(null, model);
                    });
                },

                function (respond)
                {
                    Client.getViewSource(url, function (error, data)
                    {
                        if (error)
                        {
                            respond(error);

                            return;
                        }

                        viewSource = data;

                        respond(null, viewSource);
                    });
                }
            ],
            function (error)
            {
                if (error) throw error;

                var viewTemplate = Handlebars.compile(viewSource);

                var viewMarkup = viewTemplate(model);

                if (typeof container === 'string')
                {
                    container = $(container);
                }

                container.html(viewMarkup);

                me.trigger('viewLoaded', [container]);

                if (respond) respond(null, viewMarkup);
            });
    },

    getModel: function (url, respond)
    {
        //alert('service/' + url);

        $.ajax(
        {
            url: 'service/' + url,

            dataType: 'json',

            success: function (data, status, xhr)
            {
                //console.dir(data);

                if (!data.success)
                {
                    respond('service returned without success.');

                    return;
                }
                
                respond(null, data.response);
            },

            error: function (xhr, status, error)
            {
                respond(error);
            }
        });
    },

    getViewSource: function (url, respond)
    {
        var route = null;

        for (var pattern in Routes)
        {
            var regex = new RegExp(pattern);

            if (regex.test(url))
            {
                route = Routes[pattern];

                break;
            }
        }

        if (route == null) throw 'Could not resolve route to get the view source for URL ' + url;

        //alert('view/' + route + '/_.html');

        $.ajax(
        {
            url: 'view/' + route + '/get.html',

            dataType: 'html',

            type: 'GET',

            success: function (data, status, xhr)
            {
                respond(null, data);
            },

            error: function (xhr, status, error)
            {
                respond(error);
            }
        });
    },

    post: function (url, body, respond)
    {
        $.ajax(
        {
            url: 'service/' + url,

            dataType: 'json',

            data: JSON.stringify(body),

            type: 'POST',

            success: function (data, status, xhr)
            {
                respond(null, data);
            },

            error: function (xhr, status, error)
            {
                respond(error);
            }
        });
    },

    'delete': function (url, body, respond)
    {
        $.ajax(
        {
            url: 'service/' + url,

            dataType: 'json',

            data: JSON.stringify(body),

            type: 'DELETE',

            success: function (data, status, xhr)
            {
                respond(null, data);
            },

            error: function (xhr, status, error)
            {
                respond(error);
            }
        });
    },

    on: function (eventName, handlerFunction)
    {
        $(this).on(eventName, handlerFunction);
    },

    trigger: function(eventName, argsArray)
    {
        $(this).trigger(eventName, argsArray);
    }
};

    window.addEventListener(
        'load',
        function ()
        {
            Client.loadLocationHash();
        },
        false);

    window.addEventListener(
        'hashchange',
        function ()
        {
            Client.loadLocationHash();
        },
        false);


