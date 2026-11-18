#include <windows.h>
typedef struct { int length; char* text; } course_buffer;
static volatile LONG live = 0;
__declspec(dllexport) course_buffer* __cdecl course_create(const char* value) {
    int length=0;if(!value)return 0;while(value[length])length++;
    course_buffer* buffer=(course_buffer*)HeapAlloc(GetProcessHeap(),0,sizeof(course_buffer));if(!buffer)return 0;
    buffer->text=(char*)HeapAlloc(GetProcessHeap(),0,(SIZE_T)length+1);if(!buffer->text){HeapFree(GetProcessHeap(),0,buffer);return 0;}
    for(int i=0;i<=length;i++)buffer->text[i]=value[i];buffer->length=length;InterlockedIncrement(&live);return buffer;
}
__declspec(dllexport) int __cdecl course_length(course_buffer* buffer){return buffer?buffer->length:-1;}
__declspec(dllexport) int __cdecl course_copy(course_buffer* buffer,unsigned char* target,int capacity){
    if(!buffer||!target||capacity<=buffer->length)return -1;
    for(int i=0;i<=buffer->length;i++)target[i]=(unsigned char)buffer->text[i];return buffer->length;
}
__declspec(dllexport) void __cdecl course_release(course_buffer* buffer){if(buffer){HeapFree(GetProcessHeap(),0,buffer->text);HeapFree(GetProcessHeap(),0,buffer);InterlockedDecrement(&live);}}
__declspec(dllexport) int __cdecl course_live(void){return (int)live;}
