function adjustLayout() {
    try {
        const screenHeight = window.innerHeight;
        const tableContainer = document.getElementById('tableContainer');

        if (!tableContainer) return;

        // Compute height (70% of screen height)
        const containerHeight = screenHeight * 0.7;

        // Compute width (maintain current perfect width behavior)
        const currentWidth = tableContainer.clientWidth;
        const containerWidth = currentWidth; // Keep current perfect width

        // Apply dimensions
        tableContainer.style.height = `${containerHeight}px`;
        tableContainer.style.width = `${containerWidth}px`; // Maintain current width
        tableContainer.style.overflow = 'auto'; // Add scroll if content overflows

        // Optional: Center the container
        tableContainer.style.margin = '20px auto';

        // Adjust internal elements if needed
        const textareas = document.querySelectorAll('textarea');
        textareas.forEach(ta => {
            ta.style.height = `${containerHeight * 0.15}px`; // 15% of container height
        });

    } catch (error) {
        console.error("AdjustLayout error:", error);
    }
}

// Debounced resize handler
let resizeTimer;
window.addEventListener('resize', () => {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(adjustLayout, 100);
});

// Initial setup
document.addEventListener('DOMContentLoaded', adjustLayout);