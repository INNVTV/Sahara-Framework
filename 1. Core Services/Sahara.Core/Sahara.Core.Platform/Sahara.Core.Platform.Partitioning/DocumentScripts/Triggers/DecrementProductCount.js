//Trigger to update both the Colecteion & Account Properties Docs with a new Count for Product and Total AccountDocuments on Insert

function decrementProductCount() {

    // Get current collection & selfLink
    var collection = getContext().getCollection();
    var collectionLink = collection.getSelfLink();

    // Get the document from request (the script runs as trigger, thus the input comes in request).
    //var doc = getContext().getRequest().getBody();

    getAndUpdatePropertiesDocs();

    function getAndUpdatePropertiesDocs() {

        //Get the CollectionProperties doc & update the count
        var isAccepted = collection.queryDocuments(collectionLink, "SELECT * FROM root r WHERE r.id = 'CollectionProperties'", function (err, feed, options) {
            if (err) throw err;
            if (!feed || !feed.length) throw new Error("Failed to find the collection properties document.");

            // The collction properties document.
            var collectionProperties = feed[0];

            // Update collecionProperties.DocumentCount AND ProductCount:
            collectionProperties.DocumentCount = (collectionProperties.DocumentCount - 1);
            collectionProperties.ProductCount = (collectionProperties.ProductCount - 1);

            // Update/replace the document in the store.
            var isAccepted = collection.replaceDocument(collectionProperties._self, collectionProperties, function (err) {
                if (err) throw err;
                // Note: in case concurrent updates causes conflict with ErrorCode.RETRY_WITH, we can't read the meta again 
                //       and update again because due to Snapshot isolation we will read same exact version (we are in same transaction).
                //       We have to take care of that on the client side.
            });
            if (!isAccepted) throw new Error("The call returned false.");
        });

    }
}