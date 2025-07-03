// screenAdjust.js
export function initializeScreenAdjust(dotNetHelper) {
    function updateScreenSize() {
        const width = window.innerWidth;
        const height = window.innerHeight;
        dotNetHelper.invokeMethodAsync('UpdateScreenSize', [width, height]);
    }

    window.addEventListener('resize', updateScreenSize);
    updateScreenSize();
}

export function getScreenDimensions() {
    return [window.innerWidth, window.innerHeight];
}