# =============================================
# Deploy script for VK Mini App (WebGL build)
# Now keeps API folder intact
# =============================================

$server = "212.113.104.36"
$user = "root"
$remotePath = "/var/www/vibiki"
$localBuildPath = "C:\Users\admin\Desktop\build"

# ---- 1. Remove only the build folder ----
Write-Host "Removing old build folder $remotePath/build ..." -ForegroundColor Cyan
ssh $user@$server "rm -rf $remotePath/build"

# ---- 2. Create fresh build folder ----
Write-Host "Creating fresh build folder $remotePath/build ..." -ForegroundColor Cyan
ssh $user@$server "mkdir -p $remotePath/build"

# ---- 3. Copy new files into build/ ----
Write-Host "Copying files from $localBuildPath to $remotePath/build ..." -ForegroundColor Cyan
scp -r $localBuildPath\* $user@$server`:$remotePath/build

if ($LASTEXITCODE -ne 0) {
    Write-Error "SCP failed! Check the path or connection."
    exit 1
}

# ---- 4. Set permissions ----
Write-Host "Setting permissions on $remotePath ..." -ForegroundColor Cyan
ssh $user@$server "sudo chown -R caddy:caddy $remotePath; sudo chmod -R 755 $remotePath"

Write-Host "Deployment completed successfully!" -ForegroundColor Green