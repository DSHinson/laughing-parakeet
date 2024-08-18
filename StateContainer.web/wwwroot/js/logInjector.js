window.initializeButtonClickLogger = (dotnetHelper) => {
    const logButtonClick = async (event, args) => {
        console.log(`Button clicked: ${event.target.innerText}`);

        // Call the .NET method to log the button click
        if (dotnetHelper) {
            await dotnetHelper.invokeMethodAsync('LogButtonClick', event.target.innerText);
        }
    };

    const attachClickHandler = (element) => {
        if (element.tagName === 'BUTTON' && !element.hasAttribute('logged')) {
            const originalHandler = element.onclick;

            element.onclick = function (...args) {
                logButtonClick(event, args);
                if (originalHandler) {
                    originalHandler.apply(this, args);
                }
            };

            // Add the 'logged' attribute to indicate that this button is tracked
            element.setAttribute('logged', 'true');
        }
    };

    // Attach to existing buttons
    document.querySelectorAll('button').forEach(attachClickHandler);

    // Set up a MutationObserver to handle dynamically added buttons
    const observer = new MutationObserver((mutations) => {
        mutations.forEach((mutation) => {
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    if (node.tagName === 'BUTTON') {
                        attachClickHandler(node);
                    } else {
                        node.querySelectorAll('button').forEach(attachClickHandler);
                    }
                }
            });
        });
    });

    // Start observing the document body for changes
    observer.observe(document.body, { childList: true, subtree: true });

    // Create a .NET helper instance
    window.dotnetHelper = dotnetHelper;
};
