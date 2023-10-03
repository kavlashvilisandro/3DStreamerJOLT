const socket = new WebSocket("ws://localhost:5281/stream/@{CustomFileName}");

socket.onmessage = async function(message) {
    let bytes = await blobToBytes(message.data);
    console.log(bytes);
    loadImageFromBytes(bytes);
};
function loadImageFromBytes(frameBytes) {
    const blob = new Blob([frameBytes], { type: 'image/png' });

    const dataUrl = URL.createObjectURL(blob);

    const imageElement = document.getElementById('imageElement');
    imageElement.src = dataUrl;
    imageElement.style.display = 'block';
}

async function blobToBytes(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
            const bytes = new Uint8Array(reader.result);
            resolve(bytes);
        };
        reader.onerror = (error) => {
            reject(error);
        };
        reader.readAsArrayBuffer(blob);
    });
}