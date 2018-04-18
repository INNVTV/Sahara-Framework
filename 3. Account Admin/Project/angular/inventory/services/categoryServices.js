(function () {
    'use strict';

    var serviceId = 'categoryServices';
    var urlBase = '/Categories/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', categoryServices]);

    function categoryServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            // Create methods ------------------
            createCategory: createCategory,
            createSubcategory: createSubcategory,
            createSubsubcategory: createSubsubcategory,
            createSubsubsubcategory: createSubsubsubcategory,

            // Get methods ---------------------
            getCategoryTree: getCategoryTree,
            getCategories: getCategories,
            getCategory: getCategory,
            getSubcategory: getSubcategory,
            getSubsubcategory: getSubsubcategory,
            getSubsubsubcategory: getSubsubsubcategory,
            //getCategoriesLog: getCategoriesLog,            
            //getCategoryLog: getCategoryLog,

            // Update methods ------------------
            updateCategoryName: updateCategoryName,
            updateCategoryVisibleState: updateCategoryVisibleState,
            updateSubcategoryName: updateSubcategoryName,
            updateSubcategoryVisibleState: updateSubcategoryVisibleState,
            updateSubsubcategoryName: updateSubsubcategoryName,
            updateSubsubcategoryVisibleState: updateSubsubcategoryVisibleState,
            updateSubsubsubcategoryName: updateSubsubsubcategoryName,
            updateSubsubsubcategoryVisibleState: updateSubsubsubcategoryVisibleState,

            // Update description methods ------------------
            updateCategoryDescription: updateCategoryDescription,
            updateSubcategoryDescription: updateSubcategoryDescription,
            updateSubsubcategoryDescription: updateSubsubcategoryDescription,
            updateSubsubsubcategoryDescription: updateSubsubsubcategoryDescription,


            // Ordering methods ------------------
            reorderCategories:reorderCategories,
            resetCategoryOrder: resetCategoryOrder,

            reorderSubcategories: reorderSubcategories,
            resetSubcategoryOrder: resetSubcategoryOrder,

            reorderSubsubcategories: reorderSubsubcategories,
            resetSubsubcategoryOrder: resetSubsubcategoryOrder,

            reorderSubsubsubcategories: reorderSubsubsubcategories,
            resetSubsubsubcategoryOrder: resetSubsubsubcategoryOrder,

            // Delete methods ------------------
            deleteCategory: deleteCategory,
            deleteSubcategory: deleteSubcategory,
            deleteSubsubcategory: deleteSubsubcategory,
            deleteSubsubsubcategory: deleteSubsubsubcategory,
            
        };

        return service;

        /* ==========================================
               GET Methods
        ==========================================*/

        function getCategoryTree(includeHidden) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetCategoryTree?includeHidden=' + includeHidden + cacheBuster });
        }

        function getCategories() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetCategories' + cacheBuster });
        }

        function getCategory(categoryNameKey) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetCategory?categoryNameKey=' + categoryNameKey + cacheBuster });
        }


        function getSubcategory(categoryNameKey, subcategoryNameKey) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSubcategory?categoryNameKey=' + categoryNameKey + "&subcategoryNameKey=" + subcategoryNameKey + cacheBuster });
        }

        function getSubsubcategory(categoryNameKey, subcategoryNameKey, subsubcategoryNameKey) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSubsubcategory?categoryNameKey=' + categoryNameKey + "&subcategoryNameKey=" + subcategoryNameKey + "&subsubcategoryNameKey=" + subsubcategoryNameKey + cacheBuster });
        }

        function getSubsubsubcategory(categoryNameKey, subcategoryNameKey, subsubcategoryNameKey, subsubsubcategoryNameKey) {

            var date = new Date();
            var cacheBuster = "&cb=" + date.getTime();

            return $http({ method: 'GET', url: urlBase + '/GetSubsubsubcategory?categoryNameKey=' + categoryNameKey + "&subcategoryNameKey=" + subcategoryNameKey + "&subsubcategoryNameKey=" + subsubcategoryNameKey + "&subsubsubcategoryNameKey=" + subsubsubcategoryNameKey + cacheBuster });
        }


        /* ==========================================
               POST Methods (Create)
        ==========================================*/

        function createCategory(categoryName, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateCategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryName: categoryName,
                    isVisible: isVisible
                }
            });
        }

        function createSubcategory(categoryId, subcategoryName, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateSubcategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryId: categoryId,
                    subcategoryName: subcategoryName,
                    isVisible: isVisible
                }
            });
        }

        function createSubsubcategory(subcategoryId, subsubcategoryName, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateSubsubcategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subcategoryId: subcategoryId,
                    subsubcategoryName: subsubcategoryName,
                    isVisible: isVisible
                }
            });
        }

        function createSubsubsubcategory(subsubcategoryId, subsubsubcategoryName, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/CreateSubsubsubcategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubcategoryId: subsubcategoryId,
                    subsubsubcategoryName: subsubsubcategoryName,
                    isVisible: isVisible
                }
            });
        }

        /* ==========================================
               POST Methods (Update)
        ==========================================*/

        function updateCategoryName(categoryId, newName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateCategoryName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryId: categoryId,
                    newName: newName,
                }
            });
        }

        function updateSubcategoryName(subcategoryId, newName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubcategoryName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subcategoryId: subcategoryId,
                    newName: newName,
                }
            });
        }

        function updateSubsubcategoryName(subsubcategoryId, newName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubsubcategoryName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubcategoryId: subsubcategoryId,
                    newName: newName,
                }
            });
        }

        function updateSubsubsubcategoryName(subsubsubcategoryId, newName) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubsubsubcategoryName' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubsubcategoryId: subsubsubcategoryId,
                    newName: newName,
                }
            });
        }


        /* ==========================================
               POST Methods (Update Descriptions)
        ==========================================*/

        function updateCategoryDescription(categoryId, newDescription) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateCategoryDescription' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryId: categoryId,
                    newDescription: newDescription,
                }
            });
        }

        function updateSubcategoryDescription(subcategoryId, newDescription) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubcategoryDescription' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subcategoryId: subcategoryId,
                    newDescription: newDescription,
                }
            });
        }

        function updateSubsubcategoryDescription(subsubcategoryId, newDescription) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubsubcategoryDescription' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubcategoryId: subsubcategoryId,
                    newDescription: newDescription,
                }
            });
        }

        function updateSubsubsubcategoryDescription(subsubsubcategoryId, newDescription) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubsubsubcategoryDescription' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubsubcategoryId: subsubsubcategoryId,
                    newDescription: newDescription,
                }
            });
        }

        /* ==========================================
               POST Methods (Visible State)
        ==========================================*/

        function updateCategoryVisibleState(categoryId, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateCategoryVisibleState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryId: categoryId,
                    isVisible: isVisible,
                }
            });
        }

        function updateSubcategoryVisibleState(subcategoryId, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubcategoryVisibleState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subcategoryId: subcategoryId,
                    isVisible: isVisible,
                }
            });
        }

        function updateSubsubcategoryVisibleState(subsubcategoryId, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubsubcategoryVisibleState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubcategoryId: subsubcategoryId,
                    isVisible: isVisible,
                }
            });
        }

        function updateSubsubsubcategoryVisibleState(subsubsubcategoryId, isVisible) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/UpdateSubsubsubcategoryVisibleState' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubsubcategoryId: subsubsubcategoryId,
                    isVisible: isVisible,
                }
            });
        }

        /* ==========================================
            POST Methods (Ordering)
        ==========================================*/

        function reorderCategories(categoryOrder) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ReorderCategories' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryOrder: categoryOrder,
                }
            });
        }

        function resetCategoryOrder() {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResetCategoryOrder' + cacheBuster,
                transformRequest: transformRequest,
                params: {

                }
            });
        }


        function reorderSubcategories(subcategoryOrder) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ReorderSubcategories' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subcategoryOrder: subcategoryOrder,
                }
            });
        }

        function resetSubcategoryOrder(categoryId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResetSubcategoryOrder' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryId: categoryId
                }
            });
        }
        function reorderSubsubcategories(subsubcategoryOrder) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ReorderSubsubcategories' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubcategoryOrder: subsubcategoryOrder,
                }
            });
        }

        function resetSubsubcategoryOrder(subcategoryId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResetSubsubcategoryOrder' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subcategoryId: subcategoryId
                }
            });
        }

        function reorderSubsubsubcategories(subsubsubcategoryOrder) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ReorderSubsubsubcategories' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubsubcategoryOrder: subsubsubcategoryOrder,
                }
            });
        }

        function resetSubsubsubcategoryOrder(subsubcategoryId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/ResetSubsubsubcategoryOrder' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubcategoryId: subsubcategoryId
                }
            });
        }

        /* ==========================================
               POST Methods (Delete)
        ==========================================*/

        function deleteCategory(categoryId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteCategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    categoryId: categoryId,
                }
            });
        }

        function deleteSubcategory(subcategoryId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteSubcategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subcategoryId: subcategoryId,
                }
            });
        }

        function deleteSubsubcategory(subsubcategoryId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteSubsubcategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubcategoryId: subsubcategoryId,
                }
            });
        }

        function deleteSubsubsubcategory(subsubsubcategoryId) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'POST',
                url: urlBase + '/DeleteSubsubsubcategory' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    subsubsubcategoryId: subsubsubcategoryId
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