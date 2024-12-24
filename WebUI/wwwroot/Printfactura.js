function ImprimirF() {

	Tamano = "factura_58mm.css";

	const factura = document.createElement('iframe');
	factura.style.display = 'none';
	document.body.appendChild(factura);

	const contenedorticket = document.getElementById('factura');

	const facturaContenido = '<html><head><title>Factura</title> <link rel="stylesheet" href="' + Tamano + '"></head><div>' + contenedorticket.innerHTML + '</div></html>';

	factura.contentWindow.document.open();
	factura.contentWindow.document.write(facturaContenido);
	factura.contentWindow.document.close();

	factura.onload = function () {
		factura.contentWindow.print();
	};
}