import sys
import re

# Tenta usar PyPDF2 para extrair texto
try:
	from PyPDF2 import PdfReader
except Exception:
	# PyPDF2 não disponível, sai silenciosamente
	print("")
	sys.exit(0)

if len(sys.argv) < 2:
	print("")
	sys.exit(0)

caminho = sys.argv[1]
text = ""
try:
	reader = PdfReader(caminho)
	for page in reader.pages:
		try:
			t = page.extract_text()
			if t:
				text += t + "\n"
		except Exception:
			continue
except Exception:
	print("")
	sys.exit(0)

# Procurar padrões comuns de PSA
patterns = [
	re.compile(r"PSA\s*(?:Total)?\s*[:\-]?\s*([0-9]+(?:[.,][0-9]+)?)", re.I),
	re.compile(r"PSA Total\s*[:\-]?\s*([0-9]+(?:[.,][0-9]+)?)", re.I),
	re.compile(r"PSA Livre\s*[:\-]?\s*([0-9]+(?:[.,][0-9]+)?)", re.I),
]

value = None
for pat in patterns:
	m = pat.search(text)
	if m:
		value = m.group(1)
		break

# Se não achar, tentar recuperar o primeiro número com ocorrência de 'PSA' antes
if not value:
	m = re.search(r"PSA[\s\S]{0,30}([0-9]+(?:[.,][0-9]+)?)", text, re.I)
	if m:
		value = m.group(1)

if value:
	# Normalizar para ponto
	value = value.replace(',', '.')
	print(value)
else:
	print("")
