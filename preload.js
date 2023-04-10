// preload.js
const { shell } = require('electron');
const path = require('path');

document.addEventListener('DOMContentLoaded', () => {
    // OPEN THE PROJECT CREATION TOOL:
    document.getElementById('newProject').addEventListener('click', () => {
        console.log("TODO: Make UI for project creation later");
        shell.openPath(path.join(__dirname,'bin/Release/net7.0/TerraModdedProjectSetup.exe'));
    });
});
