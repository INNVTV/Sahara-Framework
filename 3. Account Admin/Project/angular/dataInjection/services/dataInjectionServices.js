(function () {
    'use strict';

    var serviceId = 'dataInjectionServices';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', dataInjectionServices]);

    function dataInjectionServices($http) {
        // Define the functions and properties to reveal.
        var service = {


            processImage: processImage,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getData() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/Count' + cacheBuster });
        }


        /* ==========================================
               POST Methods
        ==========================================*/

        function processImage(imageId, objectType, objectId, imageGroupNameKey, imageFormatNameKey, containerName, imageFormat, type, top, left, right, bottom) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/ProcessImage' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    imageId: imageId,
                    objectType: objectType,
                    objectId: objectId,
                    imageGroupNameKey: imageGroupNameKey,
                    imageFormatNameKey: imageFormatNameKey,
                    containerName: containerName,
                    imageFormat: imageFormat,
                    type: type,
                    quality: 90,
                    top: top,
                    left: left,
                    right: right,
                    bottom: bottom,
                    title: 'Injected title...',
                    description: 'Injected description 1 2 3 4 5 6 7 8 9...',
                    brightness: 0,
                    contrast: 0,
                    saturation: 0,
                    sharpness: 0,
                    sepia: false,
                    polaroid: false,
                    greyscale: false,
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