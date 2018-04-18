(function () {
    'use strict';

    var serviceId = 'imageServices';
    //var profileUrlBase = '/Profile/Json';

    // Inject into main app module:
    angular.module('app')
        .factory(serviceId, ['$http', imageServices]);

    function imageServices($http) {
        // Define the functions and properties to reveal.
        var service = {

            //get Records -----------------------------
            getImageRecordsForObject: getImageRecordsForObject,

            //upload image to slug
            uploadIntermediaryImageForObjectRecord: uploadIntermediaryImageForObjectRecord,

            //Delete Record -----------------------------
            deleteImageRecordForObject: deleteImageRecordForObject,

            //Delete Gallery Image -----------------------------
            deleteGalleryImageForObject: deleteGalleryImageForObject,

            //Update Images -----------------------------
            updateImageTitle: updateImageTitle,
            updateImageDescription: updateImageDescription,
            updateGalleryTitle: updateGalleryTitle,
            updateGalleryDescription: updateGalleryDescription,

            //Reorder Gallery Images -----------------------------
            reorderGallery: reorderGallery,
        };

        return service;

        /* ==========================================
               Get Methods (images)
        ==========================================*/

        function getImageRecordsForObject(imageFormatGroupTypeNameKey, objectId, listingsOnly) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/ImageRecords/Json/GetImageRecordsForObject' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    imageFormatGroupTypeNameKey: imageFormatGroupTypeNameKey,
                    objectId: objectId,
                    listingsOnly: listingsOnly
                }
            });
        }

        /* ==========================================
            UPLOAD IMAGE
        ==========================================*/


        function uploadIntermediaryImageForObjectRecord(file, type, formatWidth, formatHeight) {

            var xhr = new XMLHttpRequest();

            xhr.open('post', '/Imaging/UploadIntermediaryImageForObjectRecord?type=' + type + '&formatWidth=' + formatWidth + '&formatHeight=' + formatHeight, true)

            xhr.setRequestHeader("Content-Type", "multipart/form-data");
            xhr.setRequestHeader("X-File-Name", file.fileName);
            xhr.setRequestHeader("X-File-Size", file.fileSize);
            xhr.setRequestHeader("X-File-Type", file.fileType);

            xhr.send(file);

            return xhr;
        }

        /* ==========================================
               Update Methods (images)
        ==========================================*/

        function updateImageTitle(objectType, objectId, groupNameKey, formatNameKey, newTitle) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/UpdateImageRecordTitle' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectType: objectType,
                    objectId: objectId,
                    groupNameKey: groupNameKey,
                    formatNameKey: formatNameKey,
                    newTitle: newTitle
                }
            });
        }

        function updateImageDescription(objectType, objectId, groupNameKey, formatNameKey, newDescription) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/UpdateImageRecordDescription' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectType: objectType,
                    objectId: objectId,
                    groupNameKey: groupNameKey,
                    formatNameKey: formatNameKey,
                    newDescription: newDescription
                }
            });
        }

        function updateGalleryTitle(objectType, objectId, groupNameKey, formatNameKey, imageIndex, newTitle) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/UpdateImageRecordGalleryTitle' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectType: objectType,
                    objectId: objectId,
                    groupNameKey: groupNameKey,
                    formatNameKey: formatNameKey,
                    imageIndex: imageIndex,
                    newTitle: newTitle
                }
            });
        }

        function updateGalleryDescription(objectType, objectId, groupNameKey, formatNameKey, imageIndex, newDescription) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/UpdateImageRecordGalleryDescription' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectType: objectType,
                    objectId: objectId,
                    groupNameKey: groupNameKey,
                    formatNameKey: formatNameKey,
                    imageIndex: imageIndex,
                    newDescription: newDescription
                }
            });
        }

        /* ==========================================
               Reorder Methods (images)
        ==========================================*/

        function reorderGallery(objectType, objectId, groupNameKey, formatNameKey, imageIndexOrder) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/ReorderImageGallery' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectType: objectType,
                    objectId: objectId,
                    groupNameKey: groupNameKey,
                    formatNameKey: formatNameKey,
                    imageIndexOrder: imageIndexOrder
                }
            });
        }

        /* ==========================================
               Delete/Post Methods (images)
        ==========================================*/

        function deleteImageRecordForObject(objectType, objectId, groupNameKey, formatNameKey) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/DeleteImageRecordForObject' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectType: objectType,
                    objectId: objectId,
                    groupNameKey: groupNameKey,
                    formatNameKey: formatNameKey
                }
            });
        }

        function deleteGalleryImageForObject(objectType, objectId,  groupNameKey, formatNameKey, imageIndex) {

            var date = new Date();
            var cacheBuster = "?cb=" + date.getTime();

            return $http({
                method: 'GET',
                url: '/Imaging/Json/DeleteGalleryImageForObject' + cacheBuster,
                transformRequest: transformRequest,
                params: {
                    objectType: objectType,
                    objectId: objectId,
                    groupNameKey: groupNameKey,
                    formatNameKey: formatNameKey,
                    imageIndex: imageIndex
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