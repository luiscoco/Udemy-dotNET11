window.courseInterop = { active: 0 };
export function attach(element, callback) {
    const resized = () => callback.invokeMethodAsync('Resized', window.innerWidth);
    window.addEventListener('resize', resized);
    window.courseInterop.active++;
    return {
        focus() { element.focus(); },
        dispose() {
            window.removeEventListener('resize', resized);
            window.courseInterop.active--;
        }
    };
}
