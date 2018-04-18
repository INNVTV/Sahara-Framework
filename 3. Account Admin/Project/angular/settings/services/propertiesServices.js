(function () {
    'use strict';

    var serviceId = 'propertiesServices';
    var urlBase = '/Properties/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', propertiesServices]);

    function propertiesServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Get methods:
            getProperties: getProperties,
            getPropertyTypes: getPropertyTypes,

            // Create methods:
            createProperty: createProperty,
            createPropertyValue: createPropertyValue,
            uploadSwatchImage: uploadSwatchImage,
            createSwatchValue: createSwatchValue,

            //Update methods:
            updateSortableState: updateSortableState,
            updateAppendableState: updateAppendableState,
            updatePropertyListingState: updatePropertyListingState,
            updatePropertyDetailsState: updatePropertyDetailsState,
            updateFacetInterval: updateFacetInterval,
            updateFacetableState: updateFacetableState,
            updateSymbol: updateSymbol,
            updateSymbolPlacement: updateSymbolPlacement,

            //Featured Properties
            updateFeaturedProperties: updateFeaturedProperties,
            resetFeaturedProperties: resetFeaturedProperties,

            // Delete methods:
            deleteProperty: deleteProperty,
            deletePropertyValue: deletePropertyValue,
            deleteSwatchValue: deleteSwatchValue,
        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getProperties() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetProperties' + cacheBuster });
        }

        function getPropertyTypes() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetPropertyTypes' + cacheBuster });
        }


        /* ==========================================
               POST Methods (Create)
        ==========================================*/

        function createProperty(propertyTypeNameKey, propertyName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateProperty' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyTypeNameKey: propertyTypeNameKey,
                    propertyName: propertyName,
                }
            });
        }

        function createPropertyValue(propertyName, propertyValueName) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreatePropertyValue' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyName: propertyName,
                    propertyValueName: propertyValueName,
                }
            });
        }

        //Step 1 (Swatch Creation)
        function uploadSwatchImage(file) {

            var xhr = new XMLHttpRequest();

            xhr.open('post', urlBase + '/UploadSwatchImage', true)

            xhr.setRequestHeader("Content-Type", "multipart/form-data");
            xhr.setRequestHeader("X-File-Name", file.fileName);
            xhr.setRequestHeader("X-File-Size", file.fileSize);
            xhr.setRequestHeader("X-File-Type", file.fileType);

            xhr.send(file);

            return xhr;
        }

        //Step 2 (Swatch Creation)
        function createSwatchValue(propertyName, swatchImage, swatchLabel) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateSwatchValue' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyName: propertyName,
                    swatchImage: swatchImage,
                    swatchLabel: swatchLabel
                }
            });
        }''
        /* ==========================================
            POST Methods (LISTING)
        ==========================================*/


        function updatePropertyListingState(propertyNameKey, isListing) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePropertyListingState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    isListing: isListing,
                }
            });
        }


        /* ==========================================
            POST Methods (DETAILS)
        ==========================================*/


        function updatePropertyDetailsState(propertyNameKey, isDetails) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdatePropertyDetailsState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    isDetails: isDetails,
                }
            });
        }

        /* ==========================================
               POST Methods (Update)
        ==========================================*/

        function updateSortableState(propertyNameKey, isSortable) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSortableState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    isSortable: isSortable
                }
            });
        }

        function updateAppendableState(propertyNameKey, isAppendable) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateAppendableState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    isAppendable: isAppendable
                }
            });
        }

        function updateFacetInterval(propertyNameKey, facetInterval) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateFacetInterval' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    facetInterval: facetInterval
                }
            });
        }

        function updateFacetableState(propertyNameKey, isFacetable) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateFacetableState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    isFacetable: isFacetable
                }
            });
        }

        function updateSymbol(propertyNameKey, symbol) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSymbol' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    symbol: symbol
                }
            });
        }

        function updateSymbolPlacement(propertyNameKey, symbolPlacement) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSymbolPlacement' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    symbolPlacement: symbolPlacement
                }
            });
        }

        /* ==========================================
               POST Methods (Featured)
        ==========================================*/

        function updateFeaturedProperties(featuredPropertyOrder) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateFeaturedProperties' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    featuredPropertyOrder: featuredPropertyOrder
                }
            });
        }

        function resetFeaturedProperties() {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResetFeaturedProperties' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                }
            });
        }

        /* ==========================================
               POST Methods (Delete)
        ==========================================*/

        function deleteProperty(propertyId) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteProperty' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyId: propertyId,
                }
            });
        }

        function deletePropertyValue(propertyNameKey, propertyValueNameKey) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeletePropertyValue' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    propertyValueNameKey: propertyValueNameKey,
                }
            });
        }

        function deleteSwatchValue(propertyNameKey, swatchNameKey) {
            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteSwatchValue' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    propertyNameKey: propertyNameKey,
                    swatchNameKey: swatchNameKey,
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