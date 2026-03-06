let dotNetHelper = null;

export function initialize(helper) {
    dotNetHelper = helper;
    document.addEventListener('keydown', handleKeyDown);
}

export function dispose() {
    document.removeEventListener('keydown', handleKeyDown);
    dotNetHelper = null;
}

function handleKeyDown(event) {
    if (!dotNetHelper) return;

    // Check for Ctrl/Cmd key
    const isCtrlOrCmd = event.ctrlKey || event.metaKey;

    if (isCtrlOrCmd) {
        switch (event.key.toLowerCase()) {
            case 'n':
                event.preventDefault();
                dotNetHelper.invokeMethodAsync('HandleShortcut', 'new');
                break;
            case 's':
                event.preventDefault();
                dotNetHelper.invokeMethodAsync('HandleShortcut', 'save');
                break;
            case 'p':
                event.preventDefault();
                dotNetHelper.invokeMethodAsync('HandleShortcut', 'print');
                break;
            case 'f':
                event.preventDefault();
                dotNetHelper.invokeMethodAsync('HandleShortcut', 'search');
                break;
        }
    }

    // ESC to close dialogs/modals
    if (event.key === 'Escape') {
        // MudBlazor handles this automatically
    }
}
