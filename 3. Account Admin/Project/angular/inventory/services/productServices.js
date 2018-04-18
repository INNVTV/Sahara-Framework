(function () {
    'use strict';

    var serviceId = 'productServices';
    var urlBase = '/Products/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', productServices]);

    function productServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Create methods ------------------
            createProduct: createProduct,

            // Get methods ---------------------
            //getProducts: getProducts,
            getProduct: getProduct,
            getProductById: getProductById, //<--Used by leads to get updated product info
            getProductProperties: getProductProperties,

            // Update methods ------------------
            updateProductVisibleState: updateProductVisibleState,
            updateName: updateName,

            // Reorder methods ------------------
            reorderProducts: reorderProducts,
            resetProductOrder: resetProductOrder,

            // Move methods ------------------
            moveProduct: moveProduct,

            // Delete methods ------------------
            deleteProduct:deleteProduct,

            // Property & Tag Management
            updateProductProperty: updateProductProperty,
            updateProductLocationProperty: updateProductLocationProperty,
            removeProductPropertyCollectionItem: removeProductPropertyCollectionItem,
            clearProductProperty: clearProductProperty,
            addProductTag:addProductTag,
            removeProductTag:removeProductTag,

        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        /*
        function getProducts(locationPath) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetProducts' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    locationPath: locationPath,
                }
            });
        }
*/

        function getProduct(fullyQualifiedName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetProduct' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                }
            });
        }

        function getProductById(id) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetProductById' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    id: id,
                }
            });
        }

        function getProductProperties(fullyQualifiedName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: urlBase + '/GetProductProperties' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                }
            });
        }


        /* ==========================================
               POST Methods (Create)
        ==========================================*/

        function createProduct(locationPath, productName, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateProduct' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    locationPath: locationPath,
                    productName: productName,
                    isVisible: isVisible
                }
            });
        }



        /* ==========================================
               POST Methods (Update)
        ==========================================*/

        function updateProductVisibleState(fullyQualifiedName, productName, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/updateProductVisibleState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    productName: productName,
                    isVisible: isVisible,
                }
            });
        }

        function updateName(fullyQualifiedName, oldName, newName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateProductName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    oldName: oldName,
                    newName: newName
                }
            });
        }


        /* ==========================================
               POST Methods (Visible State)
        ==========================================*/

        function updateProductState(productId, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateProductState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    productId: productId,
                    isVisible: isVisible,
                }
            });
        }


        /* ==========================================
            POST Methods (Ordering)
        ==========================================*/

        function reorderProducts(productOrder, locationPath) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ReorderProducts' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    productOrder: productOrder,
                    locationPath: locationPath,
                }
            });
        }

        function resetProductOrder(locationPath) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResetProductOrder' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    locationPath: locationPath,
                }
            });
        }


        /* ==========================================
            POST Methods (Move)
        ==========================================*/

        function moveProduct(productId, newLocationPath) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/MoveProduct' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    productId: productId,
                    newLocationPath: newLocationPath,
                }
            });
        }

        /* ==========================================
            POST Methods (Delete)
        ==========================================*/

        function deleteProduct(productId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteProduct' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    productId: productId,
                }
            });
        }

        /* ==========================================
               POST Methods (Property Management)
        ==========================================*/

        function updateProductProperty(fullyQualifiedName, productName, propertyTypeNameKey, propertyNameKey, propertyValue, updateType) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateProductProperty' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    productName: productName,
                    propertyTypeNameKey:propertyTypeNameKey,
                    propertyNameKey: propertyNameKey,
                    propertyValue: propertyValue,
                    updateType: updateType
                }
            });
        }

        function updateProductLocationProperty(fullyQualifiedName, productName, propertyTypeNameKey, propertyNameKey, name, address1, address2, city, state, postalCode, country, lat, lng) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateProductLocationProperty' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    productName: productName,
                    propertyTypeNameKey: propertyTypeNameKey,
                    propertyNameKey: propertyNameKey,
                    name: name,
                    address1: address1,
                    address2: address2,
                    city: city,
                    state: state,
                    postalCode: postalCode,
                    country: country,
                    lat: lat,
                    lng, lng
                }
            });
        }
     
        function removeProductPropertyCollectionItem(fullyQualifiedName, productName, propertyNameKey, collectionItemIndex) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/RemoveProductPropertyCollectionItem' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    productName: productName,
                    propertyNameKey: propertyNameKey,
                    collectionItemIndex: collectionItemIndex
                }
            });
        }


        function clearProductProperty(fullyQualifiedName, productName, propertyNameKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ClearProductProperty' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    productName: productName,
                    propertyNameKey: propertyNameKey
                }
            });
        }


        /* ==========================================
            POST Methods (Tag Management)
       ==========================================*/

        function addProductTag(fullyQualifiedName, productName, tagName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/AddProductTag' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    productName: productName,
                    tagName:tagName
                }
            });
        }

        function removeProductTag(fullyQualifiedName, productName, tagName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/RemoveProductTag' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    fullyQualifiedName: fullyQualifiedName,
                    productName: productName,
                    tagName:tagName
                }
            });
        }

        /* ==========================================
               POST Methods (Delete)
        ==========================================*/

        function deleteProduct(productId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteProduct' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    productId: productId,
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