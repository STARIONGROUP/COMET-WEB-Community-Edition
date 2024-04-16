/**
 * Downloads a given file by its name
 * @param {string} fileName
 * @param {any} contentStreamReference
 */
function DownloadFileFromStream(fileName, contentStreamReference) {
    (async () => {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer], { type: 'text/plain;charset=utf-8' });

        const url = URL.createObjectURL(blob);

        const anchorElement = document.createElement('a');

        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.target = "_blank";
        anchorElement.click();
        anchorElement.remove();
        URL.revokeObjectURL(url);
    })();
}