(function () {
    'use strict';

    var serviceId = 'productServices';
    var urlBase = 'https://';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', productServices]);

    function productServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getProductDetails: getProductDetails,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getProductDetails(accountNameKey, apiDomain, fullyQualifiedName) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/item/' + fullyQualifiedName + "?includeHidden=true" + cacheBuster,

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
