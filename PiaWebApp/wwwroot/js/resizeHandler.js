export function initializeResizeHandler(dotNetHelper, containerId, appContainerId) {
    const container = document.getElementById(containerId);
    const appContainer = document.getElementById(appContainerId);
    let resizeObserver;

    function handleResize() {
        try {
            const screenWidth = window.innerWidth;
            const screenHeight = window.innerHeight;
            let containerSize;

            if (screenWidth > screenHeight) {
                // Landscape - make square based on height
                containerSize = screenHeight * 0.80;
            } else {
                // Portrait - make square based on width
                containerSize = screenWidth * 0.80;
            }

            // Apply the calculated size
            container.style.width = `${containerSize}px`;
            container.style.height = `${containerSize}px`;

            // Scale font sizes proportionally
            const fontSize = Math.max(12, containerSize / 55);
            container.style.fontSize = `${fontSize}px`;

            // Notify .NET about the new dimensions
            dotNetHelper.invokeMethodAsync('UpdateDimensions', containerSize);
        } catch (error) {
            console.error('Error in handleResize:', error);
        }
    }

    // Set up ResizeObserver for more precise control
    if (typeof ResizeObserver !== 'undefined') {
        resizeObserver = new ResizeObserver(() => {
            handleResize();
        });
        resizeObserver.observe(appContainer);
    }

    // Initial setup
    handleResize();
    window.addEventListener('resize', handleResize);

    // Return cleanup function
    return {
        dispose: () => {
            window.removeEventListener('resize', handleResize);
            if (resizeObserver) {
                resizeObserver.disconnect();
            }
        }
    };
}