(function () {
    'use strict';

    var serviceId = 'accountServices';
    var urlBase = 'https://';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', accountServices]);

    function accountServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getAccount: getAccount,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getAccount(accountNameKey, apiDomain) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/account' + cacheBuster,

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
