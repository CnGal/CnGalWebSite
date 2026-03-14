export function getElementMetrics(element) {
    if (!element) {
        return { width: 0, height: 0, left: 0, top: 0 };
    }

    const rect = element.getBoundingClientRect();
    return {
        width: rect.width || 0,
        height: rect.height || 0,
        left: rect.left || 0,
        top: rect.top || 0
    };
}

export async function cropDataUrlByRect(dataUrl, x, y, width, height) {
    if (!dataUrl) {
        return { base64: null, mimeType: null };
    }

    const image = new Image();
    image.decoding = "sync";
    image.src = dataUrl;
    await image.decode();

    const naturalWidth = image.naturalWidth || image.width;
    const naturalHeight = image.naturalHeight || image.height;
    if (!naturalWidth || !naturalHeight) {
        return { base64: null, mimeType: null };
    }

    const normalizedX = Math.max(0, Math.min(1, x));
    const normalizedY = Math.max(0, Math.min(1, y));
    const normalizedW = Math.max(0.001, Math.min(1 - normalizedX, width));
    const normalizedH = Math.max(0.001, Math.min(1 - normalizedY, height));

    const sx = Math.max(0, Math.floor(normalizedX * naturalWidth));
    const sy = Math.max(0, Math.floor(normalizedY * naturalHeight));
    const sw = Math.max(1, Math.min(naturalWidth - sx, Math.floor(normalizedW * naturalWidth)));
    const sh = Math.max(1, Math.min(naturalHeight - sy, Math.floor(normalizedH * naturalHeight)));

    const canvas = document.createElement("canvas");
    canvas.width = sw;
    canvas.height = sh;
    const context = canvas.getContext("2d");
    if (!context) {
        return { base64: null, mimeType: null };
    }

    context.drawImage(image, sx, sy, sw, sh, 0, 0, sw, sh);

    const sourceMime = (dataUrl.match(/^data:(.*?);base64,/) || [])[1] || "image/jpeg";
    const outputMime = sourceMime === "image/png" || sourceMime === "image/webp" ? sourceMime : "image/jpeg";
    const outputDataUrl = canvas.toDataURL(outputMime, 0.92);
    const base64 = outputDataUrl.substring(outputDataUrl.indexOf(",") + 1);
    return { base64, mimeType: outputMime };
}
