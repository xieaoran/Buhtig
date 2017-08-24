using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Buhtig.Entities.Git;
using Buhtig.Entities.User;
using Buhtig.Resources.Strings;
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Diagrams;
using Telerik.Windows.Diagrams.Core;

namespace Buhtig.Helpers
{
    public static class ChartHelper
    {
        public static IEnumerable<CategoricalDataPoint> GenerateCodeLinesChart(object element)
        {
            ObservableCollection<GitCommit> commits;
            if (element.GetType() == typeof(Team)) commits = ((Team)element).Repo.Commits;
            else if (element.GetType() == typeof(Student)) commits = ((Student)element).Commits;
            else throw new ArgumentOutOfRangeException();

            var dataPoints = new List<CategoricalDataPoint>();
            var codeSum = 0;
            foreach (var commit in commits)
            {
                codeSum +=
                    commit.Changes.Values.Sum(
                        cL => cL.Where(c => !c.Framework && !c.Merge).Sum(c => c.Summary.LinesAdded - c.Summary.LinesDeleted));
                dataPoints.Add(new CategoricalDataPoint { Category = commit.Time, Value = codeSum });
            }
            return dataPoints;
        }

        public static IEnumerable<CategoricalDataPoint> GenerateCommitChart(object element)
        {
            ObservableCollection<GitCommit> commits;
            if (element.GetType() == typeof(Team)) commits = ((Team)element).Repo.Commits;
            else if (element.GetType() == typeof(Student)) commits = ((Student)element).Commits;
            else throw new ArgumentOutOfRangeException();

            var dataPoints = new List<CategoricalDataPoint>();
            var commitDays = new Dictionary<DateTime, int>();
            foreach (var commit in commits)
            {
                var commitDay = commit.Time - commit.Time.TimeOfDay;
                if (commitDays.ContainsKey(commitDay)) commitDays[commitDay] += 1;
                else commitDays[commitDay] = 1;
            }
            foreach (var commitDay in commitDays)
            {
                dataPoints.Add(new CategoricalDataPoint { Category = commitDay.Key, Value = commitDay.Value });
            }
            commitDays.Clear();
            return dataPoints;
        }

        public static IEnumerable<PieDataPoint> GenerateContributionChart(object element)
        {
            Team team;
            if (element.GetType() == typeof(Team)) team = (Team)element;
            else if (element.GetType() == typeof(Student)) team = ((Student)element).BelongingTeam;
            else throw new ArgumentOutOfRangeException();

            var dataPoints = new List<PieDataPoint>();
            foreach (var member in team.Members)
            {
                dataPoints.Add(new PieDataPoint { Label = member.ContributionString, Value = member.Contribution });
            }

            return dataPoints;
        }

        public static IEnumerable<object> GenerateCollaborationDiagram(object element)
        {
            Team team;
            if (element.GetType() == typeof(Team)) team = (Team)element;
            else if (element.GetType() == typeof(Student)) team = ((Student)element).BelongingTeam;
            else throw new ArgumentOutOfRangeException();

            var diagramElements = new List<object>();
            var memberShapes = new List<RadDiagramShape>();

            var radius = int.Parse(StaticConfigs.MemberRadius);
            var ratio = int.Parse(StaticConfigs.RadiusIntervalRatio);

            var center = radius * ratio;

            var unitDegrees = Math.PI * 2 / team.Members.Count;
            for (var memberIndex = 0; memberIndex < team.Members.Count; memberIndex++)
            {
                var member = team.Members[memberIndex];
                var memberShape = new RadDiagramShape
                {
                    Width = radius,
                    Height = radius,
                    Position = new Point(center + radius * ratio * Math.Cos(unitDegrees * memberIndex),
                        center + radius * ratio * Math.Sin(unitDegrees * memberIndex)),
                    Content = member.ToString(),
                    Geometry = ShapeFactory.GetShapeGeometry(CommonShapeType.EllipseShape),
                    Tag = member
                };
                diagramElements.Add(memberShape);
                memberShapes.Add(memberShape);
            }

            foreach (var collaboration in team.Collaborations)
            {
                for (var member1Index = 0; member1Index < collaboration.Members.Count; member1Index++)
                {
                    var member1 = collaboration.Members[member1Index];
                    var member1Shape = memberShapes.First(s => Equals(s.Tag, member1));
                    for (var member2Index = member1Index + 1; member2Index < collaboration.Members.Count; member2Index++)
                    {
                        var member2 = collaboration.Members[member2Index];
                        var member2Shape = memberShapes.First(s => Equals(s.Tag, member2));
                        var connection = new RadDiagramConnection
                        {
                            Source = member1Shape,
                            SourceConnectorPosition = ConnectorPosition.Auto,
                            Target = member2Shape,
                            TargetConnectorPosition = ConnectorPosition.Auto,
                            SourceCapType = CapType.Arrow1Filled,
                            TargetCapType = CapType.Arrow1Filled
                        };
                        diagramElements.Add(connection);
                    }
                }
            }

            memberShapes.Clear();

            return diagramElements;
        }
    }
}
