all: clean test2

legs16: legs16.c
	$(CC) -o legs16 legs16.c

../bfc/bfc: ../bfc/bfc.c
	$(CC) -o ../bfc/bfc ../bfc/bfc.c 

forth.img: ../bfc/bfc forth.fs
	../bfc/bfc -o forth.img forth.fs

test.dsk: test.fs
	cat test.fs /dev/zero | dd bs=256 count=630 > test.dsk conv=sync

test:	legs16 forth.img forth2.fs test.dsk
	cat forth2.fs | legs16 -sc

test2:  legs16 forth.img forth2.fs test.dsk
	cat forth2.fs | legs16 -s

disk:   mul.fs
	os9 format test.os9 -ss -t35 -e -nLegs
	os9 makdir test.os9,src 
	os9 copy mul.fs test.os9,src/mul.fs
	os9 copy test.fs test.os9,src/test.fs

clean:
	rm -f *~ legs16 forth.img
