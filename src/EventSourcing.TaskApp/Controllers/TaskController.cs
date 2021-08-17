using System;
using System.Threading.Tasks;
using EventSourcing.TaskApp.Core;
using EventSourcing.TaskApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.TaskApp.Controllers
{
    [Route("api/tasks/{id}")]
    [ApiController]
    [Consumes("application/x-www-form-urlencoded")]
    public class TaskController : ControllerBase
    {
        private readonly AggregateRepository _aggregateRepository;
        private readonly TaskRepository _taskRepository;

        public TaskController(AggregateRepository aggregateRepository, TaskRepository taskRepository)
        {
            _aggregateRepository = aggregateRepository;
            _taskRepository = taskRepository;
        }

        [HttpPost, Route("create")]
        public async Task<IActionResult> Create(Guid id, [FromForm] string title)
        {
            try
            {
                var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
                aggregate.Create(id, title, "metalsimyaci");

                await _aggregateRepository.SaveAsync(aggregate);

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }

        [HttpPatch, Route("assign")]
        public async Task<IActionResult> Assign(Guid id, [FromForm] string assignedTo)
        {
            try
            {
                var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
                aggregate.Assign(assignedTo, "metalsimyaci");

                await _aggregateRepository.SaveAsync(aggregate);

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }

        [HttpPatch, Route("move")]
        public async Task<IActionResult> Move(Guid id, [FromForm] BoardSections section)
        {
            try
            {
                var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
                aggregate.Move(section, "metalsimyaci");

                await _aggregateRepository.SaveAsync(aggregate);

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }

        [HttpPatch, Route("complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            try
            {
                var aggregate = await _aggregateRepository.LoadAsync<Core.Task>(id);
                aggregate.Complete("metalsimyaci");

                await _aggregateRepository.SaveAsync(aggregate);

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var task = await _taskRepository.Get(id);
            return Ok(task);
        }
    }
}