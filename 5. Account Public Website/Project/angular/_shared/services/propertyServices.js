(function () {
    'use strict';

    var serviceId = 'propertyServices';
    var urlBase = 'https://';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', propertyServices]);

    function propertyServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getProperties: getProperties,
            getPropertiesFiltered: getPropertiesFiltered,
        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getProperties(accountNameKey, apiDomain, type) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/properties/item?type=' + type + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },
            });
        }

        function getPropertiesFiltered(accountNameKey, apiDomain, type, filter) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/properties/item?type=' + type + "&filter=" + filter + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },
            });
        }

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
