function createProfile(definition) {
    function reset(context) {
        for (const [file, source] of Object.entries(definition.resetFiles)) {
            context.write(file, source);
        }
    }

    const actions = {
        reset: { label: 'Restore exercise', run: reset }
    };

    for (const step of definition.steps) {
        if (step.writeFiles && !Object.hasOwn(actions, 'prepare-' + step.id)) {
            Object.defineProperty(actions, 'prepare-' + step.id, {
                value: {
                    label: 'Prepare ' + step.label,
                    run(context) {
                        for (const [file, source] of Object.entries(step.writeFiles)) {
                            context.write(file, source);
                        }
                    }
                }
            });
        }
        actions[step.id] = {
            label: step.label,
            async run(context) {
                for (const [file, source] of Object.entries(step.writeFiles || {})) {
                    context.write(file, source);
                }
                for (const args of step.commands.slice(0, -1)) {
                    await context.run(args);
                }
                return context.dotnet(step.commands.at(-1));
            }
        };
    }

    return {
        number: definition.number,
        title: definition.title,
        files: definition.files,
        reset,
        actions,
        sequence: definition.steps.map(step => ({
            action: step.id,
            failure: step.failure || false,
            contains: step.contains || []
        }))
    };
}

module.exports = { createProfile };
