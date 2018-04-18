(function () {
    'use strict';

    var serviceId = 'searchServices';
    var urlBase = '/Search/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', searchServices]);

    function searchServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Product Methods 
            searchProducts: searchProducts,

            // Facet Methods 
            getFacets: getFacets,
            getSortables: getSortables,
            getFeatured: getFeatured,
        };

        return service;

        /* ==========================================
            SEARCH Methods
        ==========================================*/

        function searchProducts(text, filter, orderBy, skip, top, locationSort, locationSortString) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/Products' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    text: text,
                    filter: filter,
                    orderBy:orderBy,
                    skip: skip,
                    top: top,
                    locationSort: locationSort,
                    locationSortString: locationSortString
                }
            });
        }

        /* ==========================================
            FACET Methods
        ==========================================*/

        function getFacets() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/Facets' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    //accountNameKey: accountNameKey,
                    //filter: filter,
                }
            });
        }

        /* ==========================================
            SORTABLE Methods
        ==========================================*/

        function getSortables() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/Sortables' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    //accountNameKey: accountNameKey,
                    //filter: filter,
                }
            });
        }

        /* ==========================================
            FEATURED Methods
        ==========================================*/

        function getFeatured() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/Featured' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    //accountNameKey: accountNameKey,
                    //filter: filter,
                }
            });
        }

        /* ==========================================
                HELPERS
        ==========================================*/

        var transformRequest = function (data, headersGetter) {
            var headers = headersGetter();
            headers['Authorization'] = 'WSSE profile="UsernameToken"';
            headers['X-WSSE'] = 'UsernameToken ' + nonce
            headers['Content-Type'] = 'application/json';
        };

    }
})();