.data
absolute qword 7fffffffffffffffh 
fade1	 qword 6.0
fade2	 qword 15.0
fade3    qword 10.0
fade4	 qword 1.0
zero	 qword 0.0


.code
Noise proc  ; r8 - PermutationTablePointer r9 - GradientTablePointer x - xmm0, y -xmm1 - fastcall

movhpd xmm9, absolute					; prepare proper value in xmm9 register
movlpd xmm9, absolute					; for absolute value

movhpd xmm11, fade1						; prepare proper value in xmm11 register
movlpd xmm11, fade1						; for fade function

movhpd xmm12, fade2						; prepare proper value in xmm12 register
movlpd xmm12, fade2						; for fade function

movhpd xmm13, fade3						; prepare proper value in xmm13 register
movlpd xmm13, fade3						; for fade function

movhpd xmm14, fade4						; prepare proper value in xmm14 register
movlpd xmm14, fade4					    ; for fade function


movlhps xmm0, xmm1						; move second argument(y) to the high part of register, xmm0L - x and xmm0H Orginal Values
movapd xmm2, xmm0						; store orginal values for future computaution


roundpd xmm1, xmm0,01B					; floor the input vector, xmm1 Floored values

cvtpd2dq  xmm3, xmm1					; convert input vector to int 
subpd xmm0, xmm1						; orginal value - floored values xmm0 - vector uv

mov rbx, 256							; prepare for modulo div
pextrd eax, xmm3, 0						; get converted to int x part of input vector
xor rdx,rdx								; clean rdx register
div rbx									; divide x part to get a position in table in rdx
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table			


pextrd edx, xmm3, 1						; get converted to int y part of input vector
add rax, rdx							; add to get second hash value
xor rdx, rdx							; prepare for modulo div
div rbx									; divide first value + y part to get second index
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table
imul rax, 16							; multiple to get position in table	
movdqu xmm4, xmmword ptr[r9 + rax]		; move x part of the vector low, move y high part of the register


dppd xmm4, xmm0, 255					; dot product for gradient vector

andpd xmm0, xmm9						; absolute value for uv.x and uv.y 
movapd xmm10, xmm0						; store absolute value for fade function

mulpd xmm0, xmm11						; multiply by 6 
subpd xmm0, xmm12						; substract 15 
mulpd xmm0, xmm10						; multiply by absolute values 
addpd xmm0, xmm13						; add 10
mulpd xmm0, xmm10						; multiply three times by absolute values
mulpd xmm0, xmm10
mulpd xmm0, xmm10 

subpd xmm14, xmm0						; end of fade function
movhlps xmm15, xmm14
mulpd xmm15, xmm14						; End of Q function
mulpd xmm4, xmm15						; multply dot product by fade(x) * fade(y) end of first iteration

; needed values for future:
; xmm4 - result of first iteration
; xmm1 - flored input values
; xmm2 - orginal input vector
; r8   - permutation table pointer
; r9   - gradient table pointer
; xmm9 - value used in abslute value calculation
; xmm11- vector of 6.0, used in fade funtion
; xmm12- vector of 15.0, used in fade funtion
; xmm13- vector of 10.0, used in fade funtion

;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
movhpd xmm14, fade4						; restore vector of 1.0 in register
movlpd xmm14, fade4					    ; for fade function

movhpd xmm3, fade4						; move 1.0 to high part of xmm0 register
movlpd xmm3, zero						; move 0.0  to low part of xmm0 register, xmm0 is current corner register
addpd xmm3, xmm1						; compute ij vector
movapd xmm0, xmm2						; move orginal values to xmm0
subpd xmm0, xmm3						; compute uv vector, xmm0 - uv vector
cvtpd2dq  xmm3, xmm3					; convert ij vector to int 

mov rbx, 256							; prepare for modulo div
pextrd eax, xmm3, 0						; get converted to int x part of ij vector
xor rdx,rdx								; clean rdx register
div rbx									; divide x part to get a position in table in rdx
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table

pextrd edx, xmm3, 1						; get converted to int y part of ij vector
add rax, rdx							; add to get second hash value
xor rdx, rdx							; prepare for modulo div
div rbx									; divide first value + y part to get second index
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table
imul rax, 16							; multiple to get position in table	
movdqu xmm6, xmmword ptr[r9 + rax]		; move x part of the vector low, move y high part of the register
			
dppd xmm6, xmm0, 255					; dot product for gradient vector


andpd xmm0, xmm9						; absolute value for uv.x and uv.y 
movapd xmm10, xmm0						; store absolute value for fade function

mulpd xmm0, xmm11						; multiply by 6 
subpd xmm0, xmm12						; substract 15 
mulpd xmm0, xmm10						; multiply by absolute values 
addpd xmm0, xmm13						; add 10
mulpd xmm0, xmm10						; multiply three times by absolute values
mulpd xmm0, xmm10
mulpd xmm0, xmm10 

subpd xmm14, xmm0						; end of fade function
movhlps xmm15, xmm14
mulpd xmm15, xmm14						; End of Q function
mulpd xmm6, xmm15						; multply dot product by fade(x) * fade(y) end of first iteration

addpd xmm4, xmm6						; result of second iteration
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
movhpd xmm14, fade4						; restore vector of 1.0 in register
movlpd xmm14, fade4					    ; for fade function

movhpd xmm3, zero						; move 0.0 to high part of xmm0 register
movlpd xmm3, fade4						; move 1.0  to low part of xmm0 register, xmm0 is current corner register
addpd xmm3, xmm1						; compute ij vector
movapd xmm0, xmm2						; move orginal values to xmm0
subpd xmm0, xmm3						; compute uv vector, xmm0 - uv vector
cvtpd2dq  xmm3, xmm3					; convert ij vector to int 

mov rbx, 256							; prepare for modulo div
pextrd eax, xmm3, 0						; get converted to int x part of ij vector
xor rdx,rdx								; clean rdx register
div rbx									; divide x part to get a position in table in rdx
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table

pextrd edx, xmm3, 1						; get converted to int y part of ij vector
add rax, rdx							; add to get second hash value
xor rdx, rdx							; prepare for modulo div
div rbx									; divide first value + y part to get second index
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table
imul rax, 16							; multiple to get position in table	
movdqu xmm6, xmmword ptr[r9 + rax]		; move x part of the vector low, move y high part of the register
			
dppd xmm6, xmm0, 255					; dot product for gradient vector


andpd xmm0, xmm9						; absolute value for uv.x and uv.y 
movapd xmm10, xmm0						; store absolute value for fade function

mulpd xmm0, xmm11						; multiply by 6 
subpd xmm0, xmm12						; substract 15 
mulpd xmm0, xmm10						; multiply by absolute values 
addpd xmm0, xmm13						; add 10
mulpd xmm0, xmm10						; multiply three times by absolute values
mulpd xmm0, xmm10
mulpd xmm0, xmm10 

subpd xmm14, xmm0						; end of fade function
movhlps xmm15, xmm14
mulpd xmm15, xmm14						; End of Q function
mulpd xmm6, xmm15						; multply dot product by fade(x) * fade(y) end of first iteration

addpd xmm4, xmm6						; result of third iteration
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
movhpd xmm14, fade4						; restore vector of 1.0 in register
movlpd xmm14, fade4					    ; for fade function

movhpd xmm3, fade4						; move 0.0 to high part of xmm0 register
movlpd xmm3, fade4						; move 1.0  to low part of xmm0 register, xmm0 is current corner register
addpd xmm3, xmm1						; compute ij vector
movapd xmm0, xmm2						; move orginal values to xmm0
subpd xmm0, xmm3						; compute uv vector, xmm0 - uv vector
cvtpd2dq  xmm3, xmm3					; convert ij vector to int 

mov rbx, 256							; prepare for modulo div
pextrd eax, xmm3, 0						; get converted to int x part of ij vector
xor rdx,rdx								; clean rdx register
div rbx									; divide x part to get a position in table in rdx
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table

pextrd edx, xmm3, 1						; get converted to int y part of ij vector
add rax, rdx							; add to get second hash value
xor rdx, rdx							; prepare for modulo div
div rbx									; divide first value + y part to get second index
imul rdx, 4								; multiple to get position in table
mov eax, dword ptr[r8 + rdx]			; get value from table
imul rax, 16							; multiple to get position in table	
movdqu xmm6, xmmword ptr[r9 + rax]		; move x part of the vector low, move y high part of the register
			
dppd xmm6, xmm0, 255					; dot product for gradient vector


andpd xmm0, xmm9						; absolute value for uv.x and uv.y 
movapd xmm10, xmm0						; store absolute value for fade function

mulpd xmm0, xmm11						; multiply by 6 
subpd xmm0, xmm12						; substract 15 
mulpd xmm0, xmm10						; multiply by absolute values 
addpd xmm0, xmm13						; add 10
mulpd xmm0, xmm10						; multiply three times by absolute values
mulpd xmm0, xmm10
mulpd xmm0, xmm10 

subpd xmm14, xmm0						; end of fade function
movhlps xmm15, xmm14
mulpd xmm15, xmm14						; End of Q function
mulpd xmm6, xmm15						; multply dot product by fade(x) * fade(y) end of first iteration

addpd xmm4, xmm6						; result of forth iteration
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
movapd xmm0, xmm4						; move value to return

ret 






Noise endp 

end 