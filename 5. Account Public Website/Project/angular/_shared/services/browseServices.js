(function () {
    'use strict';

    var serviceId = 'browseServices';
    var urlBase = 'https://';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', browseServices]);

    function browseServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            browseProducts: browseProducts,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function browseProducts(accountNameKey, apiDomain, property, value, skip, take) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/browse/' + property + '/' + value + '?skip='+ skip + '&take=' + take + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },


            });

            //return $http.jsonp(urlBase + accountNameKey + '.' + apiDomain + '/account');

            //return $.ajax({
                //url: urlBase + accountNameKey + '.' + apiDomain + '/account',
                //dataType: 'JSONP',
                //jsonpCallback: 'callback',
                //type: 'GET'
            //})
        }


        /* ==========================================
               POST Methods
        ==========================================

        function updateData(id, name) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    id: id,
                    name: name
                }
            });
        }*/


        /* ==========================================
                HELPERS
        ==========================================*/

        var transformRequest = function (data, headersGetter) {
            var headers = headersGetter();
            headers['Authorization'] = 'WSSE profile="UsernameToken"';
            headers['X-WSSE'] = 'UsernameToken ' + nonce;
            headers['Content-Type'] = 'application/json';
        };

    }
})();
