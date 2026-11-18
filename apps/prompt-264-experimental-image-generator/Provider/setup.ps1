$ErrorActionPreference='Stop'
docker build -t dotnet-course/lesson264-diffusion:local $PSScriptRoot
$modelFolder=Join-Path $PSScriptRoot 'downloaded-model'
New-Item -ItemType Directory -Force $modelFolder|Out-Null
docker run --rm -e HF_HUB_DISABLE_XET=1 -v "${modelFolder}:/model" dotnet-course/lesson264-diffusion:local python -c "from huggingface_hub import snapshot_download; snapshot_download('segmind/tiny-sd',revision='cad0bd7495fa6c4bcca01b19a723dc91627fe84f',local_dir='/model')"
$env:COURSE_IMAGE_MODEL=$modelFolder
Write-Output 'Run the application in this shell; COURSE_IMAGE_MODEL selects the downloaded weights.'
