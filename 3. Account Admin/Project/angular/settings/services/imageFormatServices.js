(function () {
    'use strict';

    var serviceId = 'imageFormatServices';
    var urlBase = '/ImageFormat/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', imageFormatServices]);

    function imageFormatServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getImageGroupTypes: getImageGroupTypes,
            getImageFormats: getImageFormats,

            // Create methods:
            createImageGroup: createImageGroup,
            createImageFormat: createImageFormat,

            // Delete methods:
            deleteImageGroup: deleteImageGroup,
            deleteImageFormat: deleteImageFormat,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getImageGroupTypes() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetImageGroupTypes' + cacheBuster });
        }

        function getImageFormats(imageGroupTypeNameKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetImageFormats' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    imageGroupTypeNameKey: imageGroupTypeNameKey,
                }
            });
        }



        /* ==========================================
               POST Methods (Create)
        ==========================================*/

        function createImageGroup(imageGroupTypeNameKey, imageGroupName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateImageGroup' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    imageGroupTypeNameKey: imageGroupTypeNameKey,
                    imageGroupName: imageGroupName,
                }
            });
        }

        function createImageFormat(imageGroupTypeNameKey, imageGroupNameKey, imageFormatName, width, height, listing, gallery) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateImageFormat' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    imageGroupTypeNameKey: imageGroupTypeNameKey,
                    imageGroupNameKey: imageGroupNameKey,
                    imageFormatName: imageFormatName,
                    width: width,
                    height: height,
                    listing: listing,
                    gallery:gallery
                }
            });
        }

        /* ==========================================
            POST Methods (VISIBLITY)
        ==========================================*/



        /* ==========================================
               POST Methods (Update)
        ==========================================*/



        /* ==========================================
               POST Methods (Delete)
        ==========================================*/

        function deleteImageGroup(imageGroupTypeNameKey, imageGroupNameKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteImageGroup' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    imageGroupTypeNameKey: imageGroupTypeNameKey,
                    imageGroupNameKey: imageGroupNameKey,
                }
            });
        }

        function deleteImageFormat(imageGroupTypeNameKey, imageGroupNameKey, imageFormatNameKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteImageFormat' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    imageGroupTypeNameKey: imageGroupTypeNameKey,
                    imageGroupNameKey: imageGroupNameKey,
                    imageFormatNameKey: imageFormatNameKey,
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