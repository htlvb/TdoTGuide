export async function uploadMedia(/** @type {String[]} */ uploadUrls) {
    let errors = []
    /** @type {HTMLInputElement} */ const uploadFileElement = document.querySelector("#new-media")
    for (let i = 0; i < uploadUrls.length; i++) {
        const file = uploadFileElement.files[i]
        try {
            const response = await fetch(uploadUrls[i], { "method": "PUT", "body": file, "headers": { "Content-Type": file.type } })
            if (!response.ok) {
                throw response
            }
        }
        catch (e) {
            console.error(`Error while uploading ${file.name}`, e)
            errors.push(`${file.name} konnte nicht hochgeladen werden.`)
        }
    }
    if (errors.length > 0) {
        throw errors.join(" ");
    }
}