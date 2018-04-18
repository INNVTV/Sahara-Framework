(function () {
    'use strict';

    var serviceId = 'categoryServices';
    var urlBase = 'https://';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', categoryServices]);

    function categoryServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getCategoryTree: getCategoryTree,
            getCategories: getCategories,
            //getCategory: getCategory,
            getCategorization: getCategorization,
        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getCategoryTree(accountNameKey, apiDomain) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/categories/tree?includeImages=true' + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },
            });
        }

        function getCategories(accountNameKey, apiDomain) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/categories' + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },
            });
        }

        /*
        function getCategory(accountNameKey, apiDomain, categoryNameKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/category/' + categoryNameKey + cacheBuster,

                transformRequest: transformRequest,
                params: {
                    //id: id,
                },
            });
        }*/

        function getCategorization(accountNameKey, apiDomain, type, fullyQualifiedName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({

                method: 'GET',

                url: urlBase + accountNameKey + '.' + apiDomain + '/' + type + '/' + fullyQualifiedName + cacheBuster,

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
