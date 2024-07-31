async function uploadMediaItem(/** @type File */ file, /** @type String */ uploadUrl) {
    const response = await fetch(uploadUrl, { "method": "PUT", "body": file, "headers": { "Content-Type": file.type } })
    if (!response.ok) {
        throw response
    }
}

export async function uploadMedia(/** @type {String[]} */ uploadUrls) {
    /** @type {HTMLInputElement} */ const uploadFileElement = document.querySelector("#new-media")
    let errors = []
    let promises = []
    for (let i = 0; i < uploadUrls.length; i++) {
        const uploadPromise = uploadMediaItem(uploadFileElement.files[i], uploadUrls[i])
            .catch(error => {
                console.error(`Error while uploading ${file.name}`, e)
                errors.push(error)
            });
        promises.push(uploadPromise)
    }
    await Promise.all(promises)
    if (errors.length > 0) {
        throw errors.join(" ");
    }
}