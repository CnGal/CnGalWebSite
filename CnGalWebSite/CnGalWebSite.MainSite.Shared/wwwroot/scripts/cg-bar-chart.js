/**
 * CgBarChart — Canvas 2D 柱状图渲染
 * Supports CSS variable color strings (e.g. "var(--cg-color-primary)") via computed style resolution.
 * Data format: labels: string[], datasets: [{ label: string, data: number[], color?: string }]
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

    ctx.clearRect(0, 0, W, H);

    const computedStyle = getComputedStyle(document.body);
    const textMuted = computedStyle.getPropertyValue('--cg-color-text-muted').trim() || '#64748b';

    if (!labels || labels.length === 0 || !datasets || datasets.length === 0) {
        ctx.fillStyle = textMuted;
        ctx.font = '13px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText('暂无数据', W / 2, H / 2);
        return;
    }

    const textColor = computedStyle.getPropertyValue('--cg-color-text-primary').trim() || '#0f172a';
    const borderColor = computedStyle.getPropertyValue('--cg-color-border').trim() || '#e2e8f0';

    /** Resolve a color string that may be a CSS variable reference like "var(--cg-color-primary)". */
    function resolveColor(raw, fallback) {
        if (!raw) return fallback;
        const match = raw.match(/^var\((--[\w-]+)\)$/);
        if (match) {
            const resolved = computedStyle.getPropertyValue(match[1]).trim();
            return resolved || fallback;
        }
        return raw;
    }

    const defaultColors = [
        '#6366f1', '#22d3ee', '#f59e0b', '#ef4444', '#10b981',
        '#8b5cf6', '#ec4899', '#14b8a6', '#f97316', '#06b6d4',
    ];

    // Find global max
    let allMax = -Infinity;
    for (const ds of datasets) {
        for (const v of ds.data) {
            if (v > allMax) allMax = v;
        }
    }
    if (allMax === -Infinity) allMax = 1;
    if (allMax === 0) allMax = 1;
    const yMax = Math.ceil(allMax * 1.12);

    // Layout — guard against zero-size canvas (e.g. during first render before CSS applies)
    if (W <= 0 || H <= 0) return;

    const padL = 44, padR = 20, padT = 8, padB = 48;
    const chartW = Math.max(0, W - padL - padR);
    const chartH = Math.max(0, H - padT - padB);
    if (chartW <= 0 || chartH <= 0) return;

    // Background
    ctx.fillStyle = computedStyle.getPropertyValue('--cg-color-surface').trim() || '#ffffff';
    ctx.fillRect(0, 0, W, H);

    // Title
    if (title) {
        ctx.fillStyle = textColor;
        ctx.font = 'bold 13px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText(title, W / 2, 20);
    }

    // Grid lines
    const gridLines = 4;
    const yStep = yMax / gridLines;
    ctx.strokeStyle = borderColor;
    ctx.lineWidth = 1;
    ctx.setLineDash([3, 3]);
    for (let i = 0; i <= gridLines; i++) {
        const y = padT + (chartH / gridLines) * i;
        ctx.beginPath();
        ctx.moveTo(padL, y);
        ctx.lineTo(W - padR, y);
        ctx.stroke();
    }
    ctx.setLineDash([]);

    // Y-axis labels
    ctx.fillStyle = textMuted;
    ctx.font = '10px Inter, sans-serif';
    ctx.textAlign = 'right';
    for (let i = 0; i <= gridLines; i++) {
        const y = padT + (chartH / gridLines) * i;
        const val = yMax - yStep * i;
        ctx.fillText(Math.round(val).toString(), padL - 6, y + 4);
    }

    // Bar layout
    const groupCount = labels.length;
    const barsPerGroup = datasets.length;
    const groupWidth = chartW / groupCount;
    const barGap = Math.max(1, groupWidth * 0.15);
    const barWidth = Math.max(2, (groupWidth - barGap * (barsPerGroup + 1)) / barsPerGroup);
    const cornerRadius = Math.min(3, barWidth * 0.3);

    // Draw bars
    for (let gi = 0; gi < groupCount; gi++) {
        const groupX = padL + groupWidth * gi;

        for (let bi = 0; bi < barsPerGroup; bi++) {
            const value = datasets[bi].data[gi] || 0;
            if (value <= 0) continue;

            const barH = (value / yMax) * chartH;
            const x = groupX + barGap + (barWidth + barGap) * bi;
            const y = padT + chartH - barH;
            const color = resolveColor(datasets[bi].color, defaultColors[bi % defaultColors.length]);

            // Rounded top bar
            ctx.fillStyle = color;
            ctx.beginPath();
            if (cornerRadius > 0) {
                ctx.moveTo(x, y + cornerRadius);
                ctx.arcTo(x, y, x + cornerRadius, y, cornerRadius);
                ctx.arcTo(x + barWidth, y, x + barWidth, y + cornerRadius, cornerRadius);
                ctx.lineTo(x + barWidth, padT + chartH);
                ctx.lineTo(x, padT + chartH);
            } else {
                ctx.rect(x, y, barWidth, barH);
            }
            ctx.closePath();
            ctx.fill();

            // Value label on top
            if (barH > 14) {
                ctx.fillStyle = textColor;
                ctx.font = `bold ${Math.min(11, barWidth * 0.8)}px Inter, sans-serif`;
                ctx.textAlign = 'center';
                ctx.textBaseline = 'bottom';
                const displayVal = Number.isInteger(value) ? value.toString() : value.toFixed(1);
                ctx.fillText(displayVal, x + barWidth / 2, y - 2);
            }
        }
    }

    // X-axis labels
    ctx.fillStyle = textMuted;
    ctx.font = '10px Inter, sans-serif';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'top';
    for (let i = 0; i < groupCount; i++) {
        const x = padL + groupWidth * i + groupWidth / 2;
        const label = labels[i];
        ctx.fillText(label, x, padT + chartH + 4);
    }

    // Legend
    if (datasets.length > 1) {
        const legendY = H - 14;
        let legendX = padL;
        ctx.font = '10px Inter, sans-serif';
        for (let bi = 0; bi < datasets.length; bi++) {
            const color = resolveColor(datasets[bi].color, defaultColors[bi % defaultColors.length]);
            ctx.fillStyle = color;
            ctx.fillRect(legendX, legendY - 7, 10, 4);
            legendX += 14;
            ctx.fillStyle = textMuted;
            ctx.textAlign = 'left';
            ctx.textBaseline = 'middle';
            ctx.fillText(datasets[bi].label || '', legendX, legendY - 5);
            legendX += ctx.measureText(datasets[bi].label || '').width + 14;
        }
    }
}
