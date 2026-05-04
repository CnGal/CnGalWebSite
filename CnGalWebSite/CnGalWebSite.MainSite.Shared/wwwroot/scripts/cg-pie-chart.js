/**
 * CgPieChart — Canvas 2D 环形图渲染
 * Supports CSS variable color strings (e.g. "var(--cg-color-success)") via computed style resolution.
 * Data format: [{ name: string, value: number, color?: string }]
 */
export function render(canvasId, slices, title, innerRadius) {
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

    if (!slices || slices.length === 0) {
        ctx.fillStyle = textMuted;
        ctx.font = '13px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText('暂无数据', W / 2, H / 2);
        return;
    }

    const textColor = computedStyle.getPropertyValue('--cg-color-text-primary').trim() || '#0f172a';

    /** Resolve a color string that may be a CSS variable reference like "var(--cg-color-success)". */
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
        '#22c55e', '#f59e0b', '#ef4444',
        '#6366f1', '#22d3ee', '#8b5cf6',
        '#ec4899', '#10b981', '#f97316',
    ];

    const total = slices.reduce((sum, s) => sum + s.value, 0);
    if (total === 0) {
        ctx.fillStyle = textMuted;
        ctx.font = '13px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText('暂无数据', W / 2, H / 2);
        return;
    }

    // Layout
    const legendWidth = 110;
    const legendGap = 16;
    const chartPadding = 20;
    const chartAvailableW = Math.max(0, W - legendWidth - legendGap - chartPadding * 2);
    const chartAvailableH = H - chartPadding * 2;
    const chartSize = Math.min(chartAvailableW, chartAvailableH);
    const chartX = chartPadding + chartSize / 2;
    const chartY = H / 2;

    const outerR = chartSize / 2;
    const innerR = outerR * innerRadius;

    // Draw slices
    let startAngle = -Math.PI / 2;
    for (let i = 0; i < slices.length; i++) {
        const slice = slices[i];
        const sliceAngle = (slice.value / total) * Math.PI * 2;
        const endAngle = startAngle + sliceAngle;
        const color = resolveColor(slice.color, defaultColors[i % defaultColors.length]);

        // Fill arc
        ctx.beginPath();
        ctx.arc(chartX, chartY, outerR, startAngle, endAngle);
        ctx.arc(chartX, chartY, innerR, endAngle, startAngle, true);
        ctx.closePath();
        ctx.fillStyle = color;
        ctx.fill();

        // Hover-like highlight border
        ctx.strokeStyle = 'rgba(255,255,255,0.5)';
        ctx.lineWidth = 1.5;
        ctx.stroke();

        // Percentage label (only if slice >= 5%)
        const pct = (slice.value / total) * 100;
        if (pct >= 5) {
            const midAngle = startAngle + sliceAngle / 2;
            const labelR = innerR + (outerR - innerR) * 0.6;
            const lx = chartX + Math.cos(midAngle) * labelR;
            const ly = chartY + Math.sin(midAngle) * labelR;
            ctx.fillStyle = 'rgba(255,255,255,0.9)';
            ctx.font = `bold ${Math.max(11, Math.min(14, chartSize * 0.06))}px Inter, sans-serif`;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(Math.round(pct) + '%', lx, ly);
        }

        startAngle = endAngle;
    }

    // Center text (total)
    ctx.fillStyle = textColor;
    ctx.font = `bold ${Math.max(14, Math.min(18, chartSize * 0.09))}px Inter, sans-serif`;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(total.toString(), chartX, chartY - 6);
    ctx.fillStyle = textMuted;
    ctx.font = '11px Inter, sans-serif';
    ctx.fillText('总计', chartX, chartY + 14);

    // Legend
    const legendX = chartPadding + chartSize + legendGap;
    let legendY = chartPadding;
    const legendItemH = 22;

    for (let i = 0; i < slices.length; i++) {
        const slice = slices[i];
        const color = resolveColor(slice.color, defaultColors[i % defaultColors.length]);
        const pct = ((slice.value / total) * 100).toFixed(1);

        // Color swatch
        ctx.fillStyle = color;
        ctx.fillRect(legendX, legendY + 3, 12, 12);

        // Name
        ctx.fillStyle = textColor;
        ctx.font = '12px Inter, sans-serif';
        ctx.textAlign = 'left';
        ctx.textBaseline = 'top';
        ctx.fillText(slice.name, legendX + 18, legendY);

        // Value + percentage
        ctx.fillStyle = textMuted;
        ctx.font = '10px Inter, sans-serif';
        ctx.fillText(`${slice.value} (${pct}%)`, legendX + 18, legendY + 14);

        legendY += legendItemH + 4;
    }
}
