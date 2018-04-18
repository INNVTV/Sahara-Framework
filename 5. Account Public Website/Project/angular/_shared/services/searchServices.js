(function () {
    'use strict';

    var serviceId = 'searchServices';
    var urlBase = 'https://';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', searchServices]);

    function searchServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getFacets: getFacets,
            getSortables: getSortables,

            // Search methods:
            searchProducts: searchProducts,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getFacets(accountNameKey, apiDomain) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/search/facets' + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },


            });
        }

        function getSortables(accountNameKey, apiDomain) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/search/sortables' + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },


            });
        }

        /* ==========================================
               SEARCH Methods
        ==========================================*/

        function searchProducts(accountNameKey, apiDomain, query, filter, orderBy, skip, take) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/search/items?query=' + query + '&filter=' + filter + '&orderBy=' + orderBy + '&skip=' + skip + '&take=' + take + cacheBuster,

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
