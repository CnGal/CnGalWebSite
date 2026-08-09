const installationKey = Symbol.for('CnGalWebSite.MainSite.Shared.imeInputInstalled');
const inputStates = new WeakMap();

function getImeInput(event) {
    const target = event.target;
    return target instanceof Element
        ? target.closest('[data-cg-ime-input]')
        : null;
}

function getInputState(element) {
    let state = inputStates.get(element);
    if (!state) {
        state = {
            isComposing: false,
            suppressedFinalValue: undefined
        };
        inputStates.set(element, state);
    }
    return state;
}

function dispatchCommittedValue(element) {
    element.dispatchEvent(new CustomEvent('cg-input-commit', {
        bubbles: true,
        composed: true,
        detail: {
            value: element.value
        }
    }));
}

function installImeInputListeners() {
    document.addEventListener('compositionstart', function (event) {
        const element = getImeInput(event);
        if (!element) {
            return;
        }

        const state = getInputState(element);
        state.isComposing = true;
        state.suppressedFinalValue = undefined;
    });

    document.addEventListener('compositionend', function (event) {
        const element = getImeInput(event);
        if (!element) {
            return;
        }

        const state = getInputState(element);
        state.isComposing = false;
        state.suppressedFinalValue = element.value;
        dispatchCommittedValue(element);
    });

    document.addEventListener('input', function (event) {
        const element = getImeInput(event);
        if (!element) {
            return;
        }

        const state = getInputState(element);
        if (event.isComposing || state.isComposing || event.inputType === 'insertCompositionText') {
            return;
        }

        if (state.suppressedFinalValue !== undefined) {
            const isDuplicateFinalInput = state.suppressedFinalValue === element.value;
            state.suppressedFinalValue = undefined;
            if (isDuplicateFinalInput) {
                return;
            }
        }

        dispatchCommittedValue(element);
    });

    document.addEventListener('blur', function (event) {
        const element = getImeInput(event);
        if (!element) {
            return;
        }

        const state = getInputState(element);
        if (!state.isComposing) {
            return;
        }

        state.isComposing = false;
        state.suppressedFinalValue = element.value;
        dispatchCommittedValue(element);
    }, true);
}

export function afterWebStarted(blazor) {
    if (document[installationKey]) {
        return;
    }

    blazor.registerCustomEventType('cginputcommit', {
        browserEventName: 'cg-input-commit',
        createEventArgs: function (event) {
            return {
                value: event.detail?.value ?? event.target?.value ?? null
            };
        }
    });
    installImeInputListeners();
    document[installationKey] = true;
}
