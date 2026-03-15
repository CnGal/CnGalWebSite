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
