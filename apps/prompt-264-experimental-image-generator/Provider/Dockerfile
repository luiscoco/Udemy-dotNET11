FROM python:3.12-slim
RUN pip install --no-cache-dir torch==2.9.1 --index-url https://download.pytorch.org/whl/cpu
RUN pip install --no-cache-dir diffusers==0.35.2 transformers==4.57.1 accelerate==1.11.0 safetensors==0.6.2 pillow==12.0.0
ENV HF_HUB_DISABLE_XET=1 OMP_NUM_THREADS=4
