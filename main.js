const { app, BrowserWindow } = require('electron')

const createWindow = () => {
    const win = new BrowserWindow({
      width: 1280,
      height: 720,
      minHeight: 300,
      minWidth:820,
      autoHideMenuBar: true
    })

    win.openDevTools();
  
    win.loadFile('dist/projectsmanager.html')
}

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') app.quit()
})

app.whenReady().then(() => {
    createWindow()
  
    app.on('activate', () => {
      if (BrowserWindow.getAllWindows().length === 0) createWindow()
    })
})