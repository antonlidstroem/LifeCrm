// quill-interop.js — Thin wrapper around Quill.js for Blazor WASM
// Place in: src/LifeCrm.Web/wwwroot/quill-interop.js

window.quillInterop = (function () {
    const instances = {};

    return {
        // Initialise a Quill editor on the element with the given id.
        // Returns the initial HTML content (empty string for a new editor).
        init: function (elementId, initialHtml, dotNetRef) {
            if (instances[elementId]) {
                instances[elementId].destroy?.();
                delete instances[elementId];
            }

            const el = document.getElementById(elementId);
            if (!el) { console.warn('quillInterop.init: element not found:', elementId); return; }

            const quill = new Quill('#' + elementId, {
                theme: 'snow',
                modules: {
                    toolbar: [
                        [{ header: [1, 2, 3, false] }],
                        ['bold', 'italic', 'underline', 'strike'],
                        [{ color: [] }, { background: [] }],
                        [{ list: 'ordered' }, { list: 'bullet' }],
                        [{ align: [] }],
                        ['link'],
                        ['clean']
                    ]
                },
                placeholder: 'Skriv ditt nyhetsbrev här…'
            });

            // Set initial content if provided
            if (initialHtml && initialHtml.trim() !== '') {
                const delta = quill.clipboard.convert({ html: initialHtml });
                quill.setContents(delta, 'silent');
            }

            // Notify Blazor whenever content changes (debounced 800 ms)
            let debounceTimer;
            quill.on('text-change', function () {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(function () {
                    const html = quill.getSemanticHTML();
                    dotNetRef.invokeMethodAsync('OnQuillChanged', html);
                }, 800);
            });

            instances[elementId] = quill;
        },

        // Get current HTML content from a Quill instance
        getHtml: function (elementId) {
            const q = instances[elementId];
            return q ? q.getSemanticHTML() : '';
        },

        // Set HTML content programmatically (e.g. when loading a saved draft)
        setHtml: function (elementId, html) {
            const q = instances[elementId];
            if (!q) return;
            const delta = q.clipboard.convert({ html: html || '' });
            q.setContents(delta, 'silent');
        },

        // Destroy a Quill instance (called from Dispose)
        destroy: function (elementId) {
            if (instances[elementId]) {
                delete instances[elementId];
            }
        }
    };
})();
