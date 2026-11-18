import torch,sys,json,time,hashlib
from diffusers import DiffusionPipeline
torch.set_num_threads(4)
pipeline=DiffusionPipeline.from_pretrained('/model',torch_dtype=torch.float32,local_files_only=True)
t=time.perf_counter()
image=pipeline(sys.argv[1],num_inference_steps=12,width=256,height=256,generator=torch.Generator('cpu').manual_seed(264)).images[0]
image.save('/out/generated.png')
print(json.dumps({'actualInference':True,'model':'segmind/tiny-sd','revision':'cad0bd7495fa6c4bcca01b19a723dc91627fe84f','width':image.width,'height':image.height,'steps':12,'seconds':time.perf_counter()-t,'sha256':hashlib.sha256(open('/out/generated.png','rb').read()).hexdigest()}))
