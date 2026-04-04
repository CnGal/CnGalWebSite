/**
 * Admin chart rendering using Canvas 2D API.
 * Renders a simple line chart with multiple series.
 */
export function render(canvasId, labels, datasets, title) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    const dpr = window.devicePixelRatio || 1;
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width * dpr;
    canvas.height = rect.height * dpr;
    ctx.scale(dpr, dpr);
    const W = rect.width;
    const H = rect.height;

    // Clear
    ctx.clearRect(0, 0, W, H);

    // Palette
    const colors = [
        '#6366f1', '#22d3ee', '#f59e0b', '#ef4444', '#10b981',
        '#8b5cf6', '#ec4899', '#14b8a6', '#f97316', '#06b6d4',
    ];

    // Layout
    const padL = 60, padR = 20, padT = 40, padB = 50;
    const chartW = W - padL - padR;
    const chartH = H - padT - padB;

    if (!labels || labels.length === 0 || !datasets || datasets.length === 0) {
        ctx.fillStyle = '#888';
        ctx.font = '14px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText('暂无数据', W / 2, H / 2);
        return;
    }

    // Find global min/max
    let allMin = Infinity, allMax = -Infinity;
    for (const ds of datasets) {
        for (const v of ds.data) {
            if (v < allMin) allMin = v;
            if (v > allMax) allMax = v;
        }
    }
    if (allMin === allMax) { allMax = allMin + 1; }
    const range = allMax - allMin;
    const yMin = Math.floor(allMin - range * 0.05);
    const yMax = Math.ceil(allMax + range * 0.05);
    const yRange = yMax - yMin || 1;

    // Background
    ctx.fillStyle = '#1a1a2e';
    ctx.fillRect(0, 0, W, H);

    // Title
    if (title) {
        ctx.fillStyle = '#e0e0ff';
        ctx.font = 'bold 14px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText(title, W / 2, 24);
    }

    // Grid lines
    const gridLines = 5;
    ctx.strokeStyle = 'rgba(255,255,255,0.08)';
    ctx.lineWidth = 1;
    for (let i = 0; i <= gridLines; i++) {
        const y = padT + (chartH / gridLines) * i;
        ctx.beginPath();
        ctx.moveTo(padL, y);
        ctx.lineTo(W - padR, y);
        ctx.stroke();
    }

    // Y-axis labels
    ctx.fillStyle = '#888';
    ctx.font = '11px Inter, sans-serif';
    ctx.textAlign = 'right';
    for (let i = 0; i <= gridLines; i++) {
        const y = padT + (chartH / gridLines) * i;
        const val = yMax - (yRange / gridLines) * i;
        ctx.fillText(Math.round(val).toString(), padL - 8, y + 4);
    }

    // X-axis labels (show max ~10)
    const maxXLabels = 10;
    const step = Math.max(1, Math.floor(labels.length / maxXLabels));
    ctx.fillStyle = '#888';
    ctx.font = '10px Inter, sans-serif';
    ctx.textAlign = 'center';
    for (let i = 0; i < labels.length; i += step) {
        const x = padL + (chartW / (labels.length - 1 || 1)) * i;
        const label = labels[i].length > 5 ? labels[i].slice(5) : labels[i];
        ctx.fillText(label, x, H - padB + 16);
    }

    // Draw series
    for (let si = 0; si < datasets.length; si++) {
        const ds = datasets[si];
        const color = colors[si % colors.length];
        const pts = ds.data;

        // Line
        ctx.strokeStyle = color;
        ctx.lineWidth = 2;
        ctx.lineJoin = 'round';
        ctx.beginPath();
        for (let i = 0; i < pts.length; i++) {
            const x = padL + (chartW / (pts.length - 1 || 1)) * i;
            const y = padT + chartH - ((pts[i] - yMin) / yRange) * chartH;
            if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
        }
        ctx.stroke();

        // Fill gradient
        const grad = ctx.createLinearGradient(0, padT, 0, padT + chartH);
        grad.addColorStop(0, color + '33');
        grad.addColorStop(1, color + '05');
        ctx.fillStyle = grad;
        ctx.beginPath();
        for (let i = 0; i < pts.length; i++) {
            const x = padL + (chartW / (pts.length - 1 || 1)) * i;
            const y = padT + chartH - ((pts[i] - yMin) / yRange) * chartH;
            if (i === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
        }
        ctx.lineTo(padL + chartW, padT + chartH);
        ctx.lineTo(padL, padT + chartH);
        ctx.closePath();
        ctx.fill();
    }

    // Legend
    if (datasets.length > 1) {
        const legendY = H - 14;
        let legendX = padL;
        ctx.font = '11px Inter, sans-serif';
        for (let si = 0; si < datasets.length; si++) {
            const color = colors[si % colors.length];
            ctx.fillStyle = color;
            ctx.fillRect(legendX, legendY - 8, 12, 4);
            legendX += 16;
            ctx.fillStyle = '#ccc';
            ctx.textAlign = 'left';
            ctx.fillText(datasets[si].label || '', legendX, legendY);
            legendX += ctx.measureText(datasets[si].label || '').width + 16;
        }
    }
}
