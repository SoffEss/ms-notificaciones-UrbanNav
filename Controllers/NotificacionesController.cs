using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using ms_notificaciones.Models;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace ms_notificaciones.Controllers;

[ApiController]
[Route("[controller]")]
public class NotificacionesController : ControllerBase
{
    [Route ("correo-bienvenida")]
    [HttpPost]
    public async Task<ActionResult> EnviarCorreoBienvenida(ModeloCorreo datos){

        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(Environment.GetEnvironmentVariable("EMAIL_FROM"),Environment.GetEnvironmentVariable("NAME_FROM"));
        var subject = datos.asuntoCorreo;
        var to = new EmailAddress(datos.correoDestino, datos.nombreDestino);
        var plainTextContent = "plain text content";
        var htmlContent = datos.contenidoCorreo;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
       
        msg.SetTemplateId(Environment.GetEnvironmentVariable("WELCOME_SENDGRID_TEMPLATE_ID"));
        msg.SetTemplateData(new {
            name = datos.nombreDestino,
            mesagge = "Bienvenido a la comunidad de UrbanNav"
        });

        var response = await client.SendEmailAsync(msg);

        if(response.StatusCode == System.Net.HttpStatusCode.Accepted){
            return Ok("Correo enviado a la dirección " + datos.correoDestino);
        } else {
            return BadRequest("Error enviando el mensaje a la dirección: " + datos.correoDestino);
        }
    }

    [Route("correo-recuperacion-clave")]
    [HttpPost]
    public async Task<ActionResult> EnviarCorreoRecuperacionClave(ModeloCorreo datos)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(apiKey);

        SendGridMessage msg = this.CrearMensajeBase(datos);
        msg.SetTemplateId(Environment.GetEnvironmentVariable("WELCOME_SENDGRID_TEMPLATE_ID"));
        msg.SetTemplateData(new
        {
            name = datos.nombreDestino,
            message = "Esta es su nueva clave.. no la comparta."
        });
        var response = await client.SendEmailAsync(msg);
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            return Ok("Correo enviado a la dirección " + datos.correoDestino);
        }
        else
        {
            return BadRequest("Error enviando el mensaje a la dirección: " + datos.correoDestino);
        }
    }

    [Route("enviar-correo-2fa")]
    [HttpPost]
    public async Task<ActionResult> EnviarCorreo2fa(ModeloCorreo datos)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(apiKey);

        SendGridMessage msg = this.CrearMensajeBase(datos);
        msg.SetTemplateId(Environment.GetEnvironmentVariable("TwoFA_SENDGRID_TEMPLATE_ID"));
        msg.SetTemplateData(new
        {
            nombre = datos.nombreDestino,
            mensaje = datos.contenidoCorreo,
            asunto = datos.asuntoCorreo
        });
        var response = await client.SendEmailAsync(msg);
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            return Ok("Correo enviado a la dirección " + datos.correoDestino);
        }
        else
        {
            return BadRequest("Error enviando el mensaje a la dirección: " + datos.correoDestino);
        }
    }

    [Route("Enviar-correo-cambio-clave")]
    [HttpPost]
    public async Task<ActionResult> EnviarCorreoCambioClave(ModeloCorreo datos)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(apiKey);

        SendGridMessage msg = this.CrearMensajeBase(datos);
        msg.SetTemplateId(Environment.GetEnvironmentVariable("TwoFA_SENDGRID_TEMPLATE_ID"));
        msg.SetTemplateData(new
        {
            nombre = datos.nombreDestino,
            mensaje = datos.contenidoCorreo,
            asunto = datos.asuntoCorreo
        });
        var response = await client.SendEmailAsync(msg);
        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
        {
            return Ok("Correo enviado a la dirección " + datos.correoDestino);
        }
        else
        {
            return BadRequest("Error enviando el mensaje a la dirección: " + datos.correoDestino);
        }
    }

        // Envío de SMS

    [Route("enviar-sms")]
    [HttpPost]
    public async Task<ActionResult> EnviarSMSNuevaClave(ModeloSms datos)
    {
        var accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY_AWS");
        //mostrar en consola la acceskey
        Console.WriteLine(accessKey);
        var secretKey = Environment.GetEnvironmentVariable("SECRET_KEY_AWS");
    //mostrar en consola la secretkey
        Console.WriteLine(secretKey);
        var client = new AmazonSimpleNotificationServiceClient(accessKey, secretKey, RegionEndpoint.USEast2);
        var messageAttributes = new Dictionary<string, MessageAttributeValue>();
        var smsType = new MessageAttributeValue
        {
            DataType = "String",
            StringValue = "Transactional"
        };

        messageAttributes.Add("AWS.SNS.SMS.SMSType", smsType);

        PublishRequest request = new PublishRequest
        {
            Message = datos.contenidoMensaje,
            //mostrar en consola el mensaje que sera enviado
            
            PhoneNumber = datos.numeroDestino,
            MessageAttributes = messageAttributes
        };
        Console.WriteLine(datos.contenidoMensaje);
        Console.WriteLine(datos.numeroDestino);
        try
        {
            await client.PublishAsync(request);
            return Ok("Mensaje enviado");
        }
        catch
        {
            return BadRequest("Error enviando el sms");
        }
    }

    private SendGridMessage CrearMensajeBase(ModeloCorreo datos)
    {
        var from = new EmailAddress(Environment.GetEnvironmentVariable("EMAIL_FROM"), Environment.GetEnvironmentVariable("NAME_FROM"));
        var subject = datos.asuntoCorreo;
        var to = new EmailAddress(datos.correoDestino, datos.nombreDestino);
        var plainTextContent = datos.contenidoCorreo;
        var htmlContent = datos.contenidoCorreo;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        return msg;
    }

}